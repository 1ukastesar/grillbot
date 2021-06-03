﻿using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class AuditLogService : ServiceBase
    {
        private GrillBotContextFactory DbFactory { get; }
        private JsonSerializerSettings JsonSerializerSettings { get; }

        public AuditLogService(DiscordSocketClient client, GrillBotContextFactory dbFactory) : base(client)
        {
            DbFactory = dbFactory;

            JsonSerializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task LogExecutedCommandAsync(CommandInfo command, ICommandContext context, IResult result)
        {
            var guildId = context.Guild.Id.ToString();
            var channelId = context.Channel.Id.ToString();
            var userId = context.User.Id.ToString();

            var data = new CommandExecution(command, context.Message, result);

            using var dbContext = DbFactory.Create();

            await dbContext.InitGuildAsync(guildId);
            await dbContext.InitGuildUserAsync(guildId, userId);
            await dbContext.InitGuildChannelAsync(guildId, channelId, userId);

            var logItem = new AuditLogItem()
            {
                ChannelId = channelId,
                CreatedAt = DateTime.Now,
                Data = JsonConvert.SerializeObject(data, JsonSerializerSettings),
                GuildId = guildId,
                ProcessedUserId = userId,
                Type = AuditLogItemType.Command
            };

            await dbContext.AddAsync(logItem);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<AuditLogStatItem>> GetStatisticsAsync<TKey>(Expression<Func<AuditLogItem, TKey>> keySelector,
            Func<List<Tuple<TKey, int, DateTime, DateTime>>, IEnumerable<AuditLogStatItem>> converter)
        {
            using var dbContext = DbFactory.Create();

            var query = dbContext.AuditLogs.AsQueryable()
                .GroupBy(keySelector)
                .OrderBy(o => o.Key)
                .Select(o => new Tuple<TKey, int, DateTime, DateTime>(o.Key, o.Count(), o.Min(x => x.CreatedAt), o.Max(x => x.CreatedAt)));

            var data = await query.ToListAsync();
            return converter(data).ToList();
        }
    }
}
