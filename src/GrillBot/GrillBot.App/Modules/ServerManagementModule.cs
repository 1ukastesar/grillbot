﻿using Discord;
using Discord.Commands;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions;
using GrillBot.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules
{
    [CommandEnabledCheck("Nelze provést příkaz ze skupiny správy serveru, protože je deaktivován.")]
    public class ServerManagementModule : Infrastructure.ModuleBase
    {
        private IConfiguration Configuration { get; }

        public ServerManagementModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("clean")]
        [Summary("Smaže zprávy v příslušném kanálu. Pokud nebyl zadán kanál jako parametr, tak bude použit kanál, kde byl zavolán příkaz.")]
        public async Task CleanAsync([Name("pocet")] int take, [Name("kanal")] ITextChannel channel = null)
        {
            await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

            if (channel == null)
            {
                channel = Context.Channel as ITextChannel;
                take++;
            }

            var options = new RequestOptions()
            {
                AuditLogReason = $"Clean command from GrillBot. Executed {Context.User} in #{channel.Name}",
                RetryMode = RetryMode.AlwaysRetry,
                Timeout = 30000
            };

            var messages = (await channel.GetMessagesAsync(take, options: options).FlattenAsync())
                .Where(o => o.Id != Context.Message.Id);

            var older = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays >= 14.0);
            var newer = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays < 14.0);

            await channel.DeleteMessagesAsync(newer, options);

            foreach (var msg in older)
            {
                await msg.DeleteAsync(options);
            }

            await ReplyAsync($"Bylo úspěšně smazáno zpráv: **{messages.Count()}**\nStarších, než 2 týdny: **{older.Count()}**\nNovějších, než 2 týdny: **{newer.Count()}**");
            await Context.Message.RemoveAllReactionsAsync();
            await Context.Message.AddReactionAsync(Emojis.Ok);
        }
    }
}
