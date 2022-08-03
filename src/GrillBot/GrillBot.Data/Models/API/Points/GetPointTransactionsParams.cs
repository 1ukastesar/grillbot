﻿using System;
using System.Linq;
using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Database;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Data.Models.API.Points;

public class GetPointTransactionsParams : IQueryableModel<Database.Entity.PointsTransaction>
{
    [DiscordId]
    public string GuildId { get; set; }

    [DiscordId]
    public string UserId { get; set; }

    public RangeParams<DateTime?> AssignedAt { get; set; }
    public bool OnlyReactions { get; set; }
    public bool OnlyMessages { get; set; }
    public string MessageId { get; set; }

    /// <summary>
    /// Available: AssignedAt, User, Points
    /// Default: AssignedAt
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "AssignedAt", Descending = true };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.PointsTransaction> SetIncludes(IQueryable<Database.Entity.PointsTransaction> query)
    {
        return query
            .Include(o => o.Guild)
            .Include(o => o.GuildUser.User);
    }

    public IQueryable<Database.Entity.PointsTransaction> SetQuery(IQueryable<Database.Entity.PointsTransaction> query)
    {
        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(UserId))
            query = query.Where(o => o.UserId == UserId);

        if (AssignedAt != null)
        {
            if (AssignedAt.From != null)
                query = query.Where(o => o.AssingnedAt >= AssignedAt.From.Value);

            if (AssignedAt.To != null)
                query = query.Where(o => o.AssingnedAt < AssignedAt.To.Value);
        }

        if (OnlyReactions)
            query = query.Where(o => o.ReactionId != "");
        if (OnlyMessages)
            query = query.Where(o => o.ReactionId == "");
        if (!string.IsNullOrEmpty(MessageId))
            query = query.Where(o => o.MessageId == MessageId);

        return query;
    }

    public IQueryable<Database.Entity.PointsTransaction> SetSort(IQueryable<Database.Entity.PointsTransaction> query)
    {
        return Sort.OrderBy switch
        {
            "User" => Sort.Descending
                ? query.OrderByDescending(o => o.GuildUser.Nickname).ThenByDescending(o => o.GuildUser.User.Username).ThenByDescending(o => o.GuildUser.User.Discriminator)
                    .ThenByDescending(o => o.AssingnedAt)
                : query.OrderBy(o => o.GuildUser.Nickname).ThenBy(o => o.GuildUser.User.Username).ThenBy(o => o.GuildUser.User.Discriminator).ThenBy(o => o.AssingnedAt),
            "Points" => Sort.Descending ? query.OrderByDescending(o => o.Points).ThenByDescending(o => o.AssingnedAt) : query.OrderBy(o => o.Points).ThenBy(o => o.AssingnedAt),
            _ => Sort.Descending ? query.OrderByDescending(o => o.AssingnedAt) : query.OrderBy(o => o.AssingnedAt)
        };
    }
}
