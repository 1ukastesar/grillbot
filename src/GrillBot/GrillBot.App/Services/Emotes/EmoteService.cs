﻿using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace GrillBot.App.Services.Emotes;

public class EmoteService : ServiceBase
{
    private string CommandPrefix { get; }
    public ConcurrentBag<GuildEmote> SupportedEmotes { get; }
    private MessageCache.MessageCache MessageCache { get; }

    public EmoteService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
        MessageCache.MessageCache messageCache) : base(client, dbFactory)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        MessageCache = messageCache;

        DiscordClient.Ready += OnReadyAsync;
        DiscordClient.MessageReceived += OnMessageReceivedAsync;
        DiscordClient.GuildAvailable += OnGuildAvailableAsync;
        DiscordClient.GuildUpdated += OnGuildUpdatedAsync;
        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
        DiscordClient.ReactionAdded += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Added);
        DiscordClient.ReactionRemoved += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Removed);

        SupportedEmotes = new ConcurrentBag<GuildEmote>();
    }

    private Task OnReadyAsync()
    {
        SyncSupportedEmotes();
        return Task.CompletedTask;
    }

    private Task OnGuildAvailableAsync(SocketGuild _)
    {
        SyncSupportedEmotes();
        return Task.CompletedTask;
    }

    private Task OnGuildUpdatedAsync(SocketGuild _, SocketGuild __)
    {
        SyncSupportedEmotes();
        return Task.CompletedTask;
    }

    private void SyncSupportedEmotes()
    {
        SupportedEmotes.Clear();
        DiscordClient.Guilds.SelectMany(o => o.Emotes).Distinct().ToList().ForEach(o => SupportedEmotes.Add(o));
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (!message.TryLoadMessage(out SocketUserMessage msg)) return; // Ignore messages from bots.
        if (string.IsNullOrEmpty(message.Content)) return; // Ignore empty messages.
        if (msg.IsCommand(DiscordClient.CurrentUser, CommandPrefix)) return; // Ignore commands.
        if (msg.Channel is not SocketTextChannel _) return; // Ignore DMs.
        if (SupportedEmotes?.IsEmpty != false) return; // Ignore events when no supported emotes is available.

        var emotes = message.GetEmotesFromMessage(SupportedEmotes.ToList()).ToList();
        if (emotes.Count == 0) return;

        var userId = message.Author.Id.ToString();

        using var context = DbFactory.Create();
        await context.InitUserAsync(message.Author, CancellationToken.None);

        foreach (var emote in emotes)
        {
            var emoteId = emote.ToString();
            var dbEmote = await context.Emotes.AsQueryable().FirstOrDefaultAsync(o => o.UserId == userId && o.EmoteId == emoteId);

            if (dbEmote == null)
            {
                dbEmote = new EmoteStatisticItem()
                {
                    EmoteId = emoteId,
                    FirstOccurence = DateTime.Now,
                    UserId = userId
                };

                await context.AddAsync(dbEmote);
            }

            dbEmote.LastOccurence = DateTime.Now;
            dbEmote.UseCount++;
        }

        await context.SaveChangesAsync();
    }

    private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        if (!messageChannel.HasValue || messageChannel.Value is not SocketTextChannel _) return;
        if (SupportedEmotes?.IsEmpty != false) return;

        var msg = message.HasValue ? message.Value : MessageCache.GetMessage(message.Id);
        if (msg is not IUserMessage userMessage) return;
        if (userMessage.IsCommand(DiscordClient.CurrentUser, CommandPrefix)) return;

        var emotes = msg.GetEmotesFromMessage(SupportedEmotes.ToList()).ToList();
        if (emotes.Count == 0) return;

        var userId = msg.Author.Id.ToString();

        using var context = DbFactory.Create();
        if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == userId)) return;

        foreach (var emote in emotes)
        {
            var emoteId = emote.ToString();
            var dbEmote = await context.Emotes.AsQueryable().FirstOrDefaultAsync(o => o.EmoteId == emoteId && o.UserId == userId);
            if (dbEmote == null || dbEmote.UseCount == 0) continue;

            dbEmote.UseCount--;
        }

        await context.SaveChangesAsync();
    }

    private async Task OnReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction, ReactionEvents @event)
    {
        if (!channel.HasValue || channel.Value is not SocketTextChannel textChannel) return;
        if (SupportedEmotes?.IsEmpty != false) return;
        if (reaction.Emote is not Emote emote) return;
        if (!SupportedEmotes.Any(o => o.IsEqual(emote))) return;

        var msg = message.HasValue ? message.Value : await MessageCache.GetMessageAsync(channel.Value, message.Id);
        var user = (reaction.User.IsSpecified ? reaction.User.Value : textChannel.Guild.GetUser(reaction.UserId)) as IGuildUser;

        if (msg == null) return;
        if (msg.Author is not IGuildUser author || author.Id == reaction.UserId) return;

        using var context = DbFactory.Create();

        await context.InitUserAsync(user, CancellationToken.None);
        await context.InitUserAsync(msg.Author, CancellationToken.None);

        if (@event == ReactionEvents.Added)
        {
            await EmoteStats_OnReactionAddedAsync(context, reaction.UserId, emote);

            if (user != null)
                await Guild_OnReactionAddedAsync(context, textChannel.Guild, user, author);
        }
        else if (@event == ReactionEvents.Removed)
        {
            await EmoteStats_OnReactionRemovedAsync(context, reaction.UserId, emote);

            if (user != null)
                await Guild_OnReactionRemovedAsync(context, textChannel.Guild, user, msg.Author);
        }

        await context.SaveChangesAsync();
    }

    #region EmoteStats

    private static async Task EmoteStats_OnReactionAddedAsync(GrillBotContext context, ulong userId, Emote emote)
    {
        var strUserId = userId.ToString();
        var emoteId = emote.ToString();

        var dbEmote = await context.Emotes.AsQueryable().FirstOrDefaultAsync(o => o.UserId == strUserId && o.EmoteId == emoteId);
        if (dbEmote == null)
        {
            dbEmote = new EmoteStatisticItem()
            {
                EmoteId = emoteId,
                UserId = strUserId,
                FirstOccurence = DateTime.Now
            };

            await context.AddAsync(dbEmote);
        }

        dbEmote.UseCount++;
        dbEmote.LastOccurence = DateTime.Now;
        await context.SaveChangesAsync();
    }

    private static async Task EmoteStats_OnReactionRemovedAsync(GrillBotContext context, ulong userId, Emote emote)
    {
        var strUserId = userId.ToString();
        var emoteId = emote.ToString();

        if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == strUserId)) return;

        var dbEmote = await context.Emotes.AsQueryable().FirstOrDefaultAsync(o => o.UserId == strUserId && o.EmoteId == emoteId);
        if (dbEmote == null || dbEmote.UseCount == 0) return;

        dbEmote.UseCount--;
        await context.SaveChangesAsync();
    }

    #endregion

    #region GivenAndObtainedEmotes

    private static async Task Guild_OnReactionAddedAsync(GrillBotContext context, SocketGuild guild, IGuildUser user, IGuildUser messageAuthor)
    {
        var guildId = guild.Id.ToString();
        var authorUserId = messageAuthor.Id.ToString();

        await context.InitGuildAsync(guild, CancellationToken.None);
        var reactingUser = await context.GuildUsers.AsQueryable().FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());
        if (reactingUser == null)
        {
            reactingUser = GuildUser.FromDiscord(guild, user);
            await context.AddAsync(reactingUser);
        }

        var authorUser = await context.GuildUsers.AsQueryable().FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == authorUserId);
        if (authorUser == null)
        {
            authorUser = GuildUser.FromDiscord(guild, messageAuthor);
            await context.AddAsync(authorUser);
        }

        authorUser.ObtainedReactions++;
        reactingUser.GivenReactions++;
    }

    private static async Task Guild_OnReactionRemovedAsync(GrillBotContext context, SocketGuild guild, IUser user, IUser messageAuthor)
    {
        var guildId = guild.Id.ToString();
        var userId = user.Id.ToString();
        var authorUserId = messageAuthor.Id.ToString();

        if (!await context.Guilds.AsQueryable().AnyAsync(o => o.Id == guildId)) return;
        if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == userId) && !await context.Users.AsQueryable().AnyAsync(o => o.Id == authorUserId)) return;

        var reactingUser = await context.GuildUsers.AsQueryable().FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);
        if (reactingUser?.GivenReactions > 0)
            reactingUser.GivenReactions--;

        var authorUser = await context.GuildUsers.AsQueryable().FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == authorUserId);
        if (authorUser?.ObtainedReactions > 0)
            authorUser.ObtainedReactions--;
    }

    #endregion
}