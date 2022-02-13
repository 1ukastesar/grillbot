﻿using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog.Events;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Helpers;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public partial class AuditLogService : ServiceBase
{
    public static JsonSerializerSettings JsonSerializerSettings { get; }
    private MessageCache.MessageCache MessageCache { get; }
    private FileStorageFactory FileStorageFactory { get; }

    private DateTime NextAllowedChannelUpdateEvent { get; set; }
    private DateTime NextAllowedRoleUpdateEvent { get; set; }

    static AuditLogService()
    {
        JsonSerializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    public AuditLogService(DiscordSocketClient client, GrillBotContextFactory dbFactory, MessageCache.MessageCache cache,
        FileStorageFactory storageFactory, DiscordInitializationService initializationService) : base(client, dbFactory, initializationService)
    {
        MessageCache = cache;

        DiscordClient.UserLeft += (guild, user) => HandleEventAsync(new UserLeftEvent(this, guild, user));
        DiscordClient.UserJoined += user => HandleEventAsync(new UserJoinedEvent(this, user));
        DiscordClient.MessageUpdated += (before, after, channel) => HandleEventAsync(new MessageEditedEvent(this, before, after, channel, MessageCache, DiscordClient));
        DiscordClient.MessageDeleted += (message, channel) => HandleEventAsync(new MessageDeletedEvent(this, message, channel, MessageCache, FileStorageFactory));

        DiscordClient.ChannelCreated += channel => HandleEventAsync(new ChannelCreatedEvent(this, channel));
        DiscordClient.ChannelDestroyed += channel => HandleEventAsync(new ChannelDeletedEvent(this, channel));
        DiscordClient.ChannelUpdated += (before, after) => HandleEventAsync(new ChannelUpdatedEvent(this, before, after));
        DiscordClient.ChannelUpdated += async (_, after) =>
        {
            await HandleEventAsync(new OverwriteChangedEvent(this, after, NextAllowedChannelUpdateEvent, DbFactory));
            NextAllowedChannelUpdateEvent = DateTime.Now.AddMinutes(1);
        };
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new EmotesUpdatedEvent(this, before, after));
        DiscordClient.GuildUpdated += (before, after) => HandleEventAsync(new GuildUpdatedEvent(this, before, after));

        DiscordClient.UserUnbanned += (user, guild) => HandleEventAsync(new UserUnbannedEvent(this, guild, user));
        DiscordClient.GuildMemberUpdated += (before, after) => HandleEventAsync(new MemberUpdatedEvent(this, before, after));
        DiscordClient.GuildMemberUpdated += async (before, after) =>
        {
            var @event = new MemberRolesUpdatedEvent(this, before, after, NextAllowedRoleUpdateEvent, DbFactory);
            await HandleEventAsync(@event);
            if (@event.Finished) NextAllowedRoleUpdateEvent = DateTime.Now.AddSeconds(30);
        };
        DiscordClient.ThreadDeleted += thread => HandleEventAsync(new ThreadDeletedEvent(this, thread));

        // TODO: Impelement audit log download after restart.

        FileStorageFactory = storageFactory;
    }

    /// <summary>
    /// Tries find guild from channel. If channel is DM method will return null;
    /// If channel is null and channelId is filled (typical usage for <see cref="Cacheable{TEntity, TId}"/>) method tries find guild with database data.
    /// </summary>
    public async Task<IGuild> GetGuildFromChannelAsync(IChannel channel, ulong channelId)
    {
        if (channel is IDMChannel) return null; // Direct messages
        if (channel is IGuildChannel guildChannel) return guildChannel.Guild;
        if (channel == null && channelId == default) return null;

        using var dbContext = DbFactory.Create();

        var guildId = await dbContext.Channels
            .Where(o => o.ChannelId == channelId.ToString())
            .Select(o => o.GuildId)
            .FirstOrDefaultAsync();

        return string.IsNullOrEmpty(guildId) ? null : DiscordClient.GetGuild(Convert.ToUInt64(guildId));
    }

    /// <summary>
    /// Stores new item in log. Method will check relationships in database and create if some will be required.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="guild"></param>
    /// <param name="channel"></param>
    /// <param name="processedUser"></param>
    /// <param name="data"></param>
    /// <param name="auditLogItemId">ID of discord audit log record. Allowed types are ulong?, string or null. Otherwise method throws <see cref="NotSupportedException"/></param>
    /// <param name="createdAt"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="attachments"></param>
    public async Task StoreItemAsync(AuditLogItemType type, IGuild guild, IChannel channel, IUser processedUser, string data, object auditLogItemId = null,
        DateTime? createdAt = null, CancellationToken? cancellationToken = null, List<AuditLogFileMeta> attachments = null)
    {
        AuditLogItem entity;
        if (auditLogItemId is null)
            entity = AuditLogItem.Create(type, guild, channel, processedUser, data, createdAt);
        else if (auditLogItemId is string _auditLogItemId)
            entity = AuditLogItem.Create(type, guild, channel, processedUser, data, _auditLogItemId, createdAt);
        else if (auditLogItemId is ulong __auditLogItemId)
            entity = AuditLogItem.Create(type, guild, channel, processedUser, data, __auditLogItemId, createdAt);
        else
            throw new NotSupportedException("Unsupported type Discord audit log item ID.");

        attachments?.ForEach(a => entity.Files.Add(a));
        using var dbContext = DbFactory.Create();

        if (processedUser != null)
            await dbContext.InitUserAsync(processedUser, cancellationToken ?? CancellationToken.None);

        if (guild != null)
        {
            await dbContext.InitGuildAsync(guild, cancellationToken ?? CancellationToken.None);

            if (processedUser != null)
            {
                if (processedUser is not IGuildUser guildUser)
                    guildUser = await guild.GetUserAsync(processedUser.Id);

                if (guildUser != null)
                    await dbContext.InitGuildUserAsync(guild, guildUser, cancellationToken ?? CancellationToken.None);
            }

            if (channel != null)
            {
                var channelType = DiscordHelper.GetChannelType(channel);
                await dbContext.InitGuildChannelAsync(guild, channel, channelType.Value, cancellationToken ?? CancellationToken.None);
            }
        }

        await dbContext.AddAsync(entity, cancellationToken ?? CancellationToken.None);
        await dbContext.SaveChangesAsync(cancellationToken ?? CancellationToken.None);
    }

    private async Task<bool> CanExecuteEvent(Func<Task<bool>> eventSpecificCheck = null)
    {
        if (!InitializationService.Get()) return false;
        if (eventSpecificCheck == null) return true;

        return await eventSpecificCheck();
    }

    private async Task HandleEventAsync(AuditEventBase @event)
    {
        if (await CanExecuteEvent(@event.CanProcessAsync))
            await @event.ProcessAsync();
    }

    public Task LogExecutedCommandAsync(CommandInfo command, ICommandContext context, global::Discord.Commands.IResult result)
        => HandleEventAsync(new ExecutedCommandEvent(this, command, context, result));

    public Task LogExecutedInteractionCommandAsync(ICommandInfo command, IInteractionContext context, global::Discord.Interactions.IResult result)
        => HandleEventAsync(new ExecutedInteractionCommandEvent(this, command, context, result));

    public async Task<bool> RemoveItemAsync(long id, CancellationToken cancellationToken)
    {
        using var context = DbFactory.Create();

        var item = await context.AuditLogs
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (item == null) return false;
        if (item.Files.Count > 0)
        {
            var storage = FileStorageFactory.Create("Audit");

            foreach (var file in item.Files)
            {
                var fileInfo = await storage.GetFileInfoAsync("DeletedAttachments", file.Filename);
                if (!fileInfo.Exists) continue;

                fileInfo.Delete();
            }

            context.RemoveRange(item.Files);
        }

        context.Remove(item);
        return (await context.SaveChangesAsync(cancellationToken)) > 0;
    }

    /// <summary>
    /// Gets IDs of audit log in discord.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="guild"></param>
    /// <param name="channel"></param>
    /// <param name="types"></param>
    /// <param name="after"></param>
    public async Task<List<ulong>> GetDiscordAuditLogIdsAsync(GrillBotContext dbContext, IGuild guild, IChannel channel, AuditLogItemType[] types, DateTime after)
    {
        var baseQuery = dbContext.AuditLogs.AsNoTracking()
            .Where(o => o.DiscordAuditLogItemId != null && o.CreatedAt >= after);

        if (guild != null)
            baseQuery = baseQuery.Where(o => o.GuildId == guild.Id.ToString());

        if (channel != null)
            baseQuery = baseQuery.Where(o => o.ChannelId == channel.Id.ToString());

        if (types?.Length > 0)
            baseQuery = baseQuery.Where(o => types.Contains(o.Type));

        var idsQuery = baseQuery.Select(o => o.DiscordAuditLogItemId).AsQueryable();
        var ids = await idsQuery.ToListAsync();
        return ids
            .SelectMany(o => o.Split(','))
            .Select(o => Convert.ToUInt64(o))
            .Distinct()
            .ToList();
    }
}
