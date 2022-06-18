﻿using GrillBot.App.Services.Permissions.Models;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Commands = Discord.Commands;
using Interactions = Discord.Interactions;

namespace GrillBot.App.Services.Permissions;

public class PermissionsService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IServiceProvider ServiceProvider { get; }

    public PermissionsService(GrillBotDatabaseBuilder databaseBuilder, IServiceProvider serviceProvider)
    {
        DatabaseBuilder = databaseBuilder;
        ServiceProvider = serviceProvider;
    }

    public async Task<PermsCheckResult> CheckPermissionsAsync(CheckRequestBase request)
    {
        request.FixImplicitPermissions();

        await using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserAsync(request.User);

        return new PermsCheckResult
        {
            ChannelDisabled = await CheckChannelDisabledAsync(repository, request),
            ContextCheck = CheckContext(request),
            ChannelPermissions = await CheckChannelPermissionsAsync(request),
            GuildPermissions = await CheckGuildPermissionsAsync(request),
            BoosterAllowed = CheckServerBooster(request),
            IsAdmin = user?.HaveFlags(UserFlags.BotAdmin) ?? false,
            ExplicitAllow = await CheckExplicitAllowAsync(repository, request),
            ExplicitBan = (user?.HaveFlags(UserFlags.CommandsDisabled) ?? false) || await CheckExplicitBansAsync(repository, request)
        };
    }

    private static async Task<bool> CheckChannelDisabledAsync(GrillBotRepository repository, CheckRequestBase request)
    {
        var channelId = request.Channel is IThreadChannel { CategoryId: { } } thread
            ? thread.CategoryId.Value
            : request.Channel.Id;

        var channel = await repository.Channel.FindChannelByIdAsync(channelId, request.Guild?.Id);
        return channel?.HasFlag(ChannelFlags.CommandsDisabled) ?? false;
    }

    private static bool? CheckContext(CheckRequestBase request)
    {
        if (request is InteractionsCheckRequest) return true; // Interactions are registered only to guilds.

        var checkRequest = (CommandsCheckRequest)request;
        if (checkRequest.Context == null) return null; // Command not depend on the context (Guild/DMs).

        // Command allows only DMs or only Guilds.
        return (checkRequest.Context == Commands.ContextType.DM && request.Guild == null) ||
               (checkRequest.Context == Commands.ContextType.Guild && request.Guild != null);
    }

    private async Task<bool?> CheckChannelPermissionsAsync(CheckRequestBase request)
    {
        if (request.ChannelPermissions == null) return null; // Command not depend on the channel permissions.

        var commandsRequest = request as CommandsCheckRequest;
        foreach (var perm in request.ChannelPermissions)
        {
            if (commandsRequest == null)
            {
                var checkRequest = (InteractionsCheckRequest)request;
                var attribute = new Interactions.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckRequirementsAsync(checkRequest.InteractionContext, checkRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
            else
            {
                var attribute = new Commands.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckPermissionsAsync(commandsRequest.CommandContext, commandsRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
        }

        return true;
    }

    private async Task<bool?> CheckGuildPermissionsAsync(CheckRequestBase request)
    {
        if (request.GuildPermissions == null) return null; // Command not depend on the guild permissions.

        var commandsRequest = request as CommandsCheckRequest;
        foreach (var perm in request.GuildPermissions)
        {
            if (commandsRequest == null)
            {
                var checkRequest = (InteractionsCheckRequest)request;
                var attribute = new Interactions.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckRequirementsAsync(checkRequest.InteractionContext, checkRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
            else
            {
                var attribute = new Commands.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckPermissionsAsync(commandsRequest.CommandContext, commandsRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
        }

        return true;
    }

    private static bool? CheckServerBooster(CheckRequestBase request)
    {
        if (!request.AllowBooster) return null;

        return request.User is SocketGuildUser user && user.Roles.Any(o => o.Tags?.IsPremiumSubscriberRole == true);
    }

    private static async Task<bool?> CheckExplicitAllowAsync(GrillBotRepository repository, CheckRequestBase request)
    {
        var permissions = await repository.Permissions.GetAllowedPermissionsForCommand(request.CommandName);
        if (permissions.Count == 0)
            return null;

        if (permissions.Any(o => !o.IsRole && o.TargetId == request.User.Id.ToString()))
            return true; // Explicit allow permission for user.

        return request.User is IGuildUser user && user.RoleIds.Any(roleId => permissions.Any(x => x.IsRole && x.TargetId == roleId.ToString()));
    }

    private static async Task<bool> CheckExplicitBansAsync(GrillBotRepository repository, CheckRequestBase request)
    {
        return await repository.Permissions.ExistsBannedCommandForUser(request.CommandName, request.User);
    }
}
