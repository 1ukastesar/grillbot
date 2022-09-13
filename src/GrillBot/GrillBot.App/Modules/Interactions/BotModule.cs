﻿using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Extensions.Discord;
using System.Diagnostics;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Common.Managers;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("bot", "Bot information and configuration commands.")]
public class BotModule : InteractionsModuleBase
{
    public BotModule(LocalizationManager localization) : base(localization)
    {
    }

    [SlashCommand("info", "Bot info")]
    public async Task BotInfoAsync()
    {
        var culture = new CultureInfo(Context.Interaction.UserLocale);
        var process = Process.GetCurrentProcess();
        var color = Context.Guild == null
            ? Color.Default
            : Context.Guild.CurrentUser.GetHighestRole(true)?.Color ?? Color.Default;
        var user = (IUser)Context.Guild?.CurrentUser ?? Context.Client.CurrentUser;

        var embed = new EmbedBuilder()
            .WithTitle(user.GetFullName())
            .WithThumbnailUrl(user.GetUserAvatarUrl())
            .AddField(GetLocale(nameof(BotInfoAsync), "CreatedAt"), user.CreatedAt.LocalDateTime.Humanize(culture: culture))
            .AddField(GetLocale(nameof(BotInfoAsync), "Uptime"), (DateTime.Now - process.StartTime).Humanize(culture: culture, maxUnit: TimeUnit.Day))
            .AddField(GetLocale(nameof(BotInfoAsync), "Repository"), "https://gitlab.com/grillbot")
            .AddField(GetLocale(nameof(BotInfoAsync), "Documentation"), "https://docs.grillbot.cloud/")
            .AddField(GetLocale(nameof(BotInfoAsync), "Swagger"), "https://grillbot.cloud/swagger")
            .AddField(GetLocale(nameof(BotInfoAsync), "PrivateAdmin"), "https://grillbot.cloud")
            .AddField(GetLocale(nameof(BotInfoAsync), "PublicAdmin"), "https://public.grillbot.cloud/")
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithFooter(Context.User)
            .Build();

        await SetResponseAsync(embed: embed);
    }

    [Group("selfunverify", "Configuring selfunverify.")]
    public class SelfUnverifyConfig : InteractionsModuleBase
    {
        private SelfunverifyService Service { get; }

        public SelfUnverifyConfig(SelfunverifyService service, LocalizationManager localization) : base(localization)
        {
            Service = service;
        }

        [SlashCommand("list-keepables", "List of allowable accesses when selfunverify")]
        public async Task ListAsync(string group = null)
        {
            var data = await Service.GetKeepablesAsync(group);

            if (data.Count == 0)
            {
                await SetResponseAsync(GetLocale(nameof(ListAsync), "NoKeepables"));
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter(Context.User)
                .WithTitle(GetLocale(nameof(ListAsync), "Title"));

            foreach (var grp in data.GroupBy(o => string.Join("|", o.Value)))
            {
                string fieldGroupResult;
                var keys = string.Join(", ", grp.Select(o => o.Key == "_" ? GetLocale(nameof(ListAsync), "Other") : o.Key));

                var fieldGroupBuilder = new StringBuilder();
                foreach (var item in grp.First().Value)
                {
                    if (fieldGroupBuilder.Length + item.Length >= EmbedFieldBuilder.MaxFieldValueLength)
                    {
                        fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                        embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[..^1] : fieldGroupResult);
                        fieldGroupBuilder.Clear();
                    }
                    else
                    {
                        fieldGroupBuilder.Append(item).Append(", ");
                    }
                }

                if (fieldGroupBuilder.Length <= 0)
                    continue;

                fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[..^1] : fieldGroupResult);
                fieldGroupBuilder.Clear();
            }

            await SetResponseAsync(embed: embed.Build());
        }
    }
}
