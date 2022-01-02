﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Implementations.Help;

public class HelpReactionHandler : ReactionEventHandler
{
    private CommandService CommandService { get; }
    private DiscordSocketClient DiscordClient { get; }
    private IServiceProvider Provider { get; }
    private string CommandPrefix { get; }

    public HelpReactionHandler(CommandService commandService, DiscordSocketClient discordClient, IServiceProvider provider, IConfiguration configuration)
    {
        CommandService = commandService;
        DiscordClient = discordClient;
        Provider = provider;
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<HelpMetadata>(message, emote, out var embed, out var metadata)) return false;

        var context = new CommandContext(DiscordClient, message.ReferencedMessage);
        var availableModules = await CommandService.Modules
            .Where(o => o.Commands.Count > 0)
            .FindAllAsync(async mod => (await mod.GetExecutableCommandsAsync(context, Provider)).Count > 0);

        int maxPages = Math.Min(metadata.PagesCount, availableModules.Count);
        int newPage = GetPageNumber(metadata.Page, maxPages, emote);
        if (newPage == metadata.Page) return false;

        var module = availableModules[newPage];

        var resultEmebd = await new EmbedBuilder()
            .WithHelpModuleAsync(module, context, Provider, maxPages, CommandPrefix, newPage);
        await message.ModifyAsync(o => o.Embed = resultEmebd.Build());

        if (!context.IsPrivate)
            await message.RemoveReactionAsync(emote, user);
        return true;
    }
}
