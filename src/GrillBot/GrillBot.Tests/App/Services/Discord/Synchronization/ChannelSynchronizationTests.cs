﻿using Discord;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Database.Entity;
using GrillBot.Tests.Common;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class ChannelSynchronizationTests : ServiceTest<ChannelSynchronization>
{
    protected override ChannelSynchronization CreateService()
    {
        return new ChannelSynchronization(DbFactory);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_ChannelNotFound()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_Ok_WithoutThreads()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, channel, ChannelType.Text));
        await DbContext.SaveChangesAsync();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_Ok()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();
        var thread = new ThreadBuilder()
            .SetId(Consts.ThreadId)
            .SetName(Consts.ThreadName)
            .SetGuild(guild)
            .SetType(ThreadType.PrivateThread)
            .Build();

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, channel, ChannelType.Text));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, thread, ChannelType.PrivateThread));
        await DbContext.SaveChangesAsync();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ThreadDeletedAsync_NotFound()
    {
        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .Build();

        var thread = new ThreadBuilder()
            .SetId(Consts.ThreadId).SetName(Consts.ThreadName)
            .SetGuild(guild)
            .Build();

        await Service.ThreadDeletedAsync(thread);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ThreadDeletedAsync_Ok()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();
        var thread = new ThreadBuilder()
            .SetId(Consts.ThreadId).SetName(Consts.ThreadName)
            .SetGuild(guild)
            .Build();

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, channel, ChannelType.Text));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, thread, ChannelType.PrivateThread));
        await DbContext.SaveChangesAsync();

        await Service.ThreadDeletedAsync(thread);
        Assert.IsTrue(true);
    }
}