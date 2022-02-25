﻿using Discord;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyLoggerTests : ServiceTest<UnverifyLogger>
{
    protected override UnverifyLogger CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();

        return new UnverifyLogger(discordClient, dbFactory);
    }

    [TestMethod]
    public async Task LogUnverifyAsync()
    {
        var guildUser = DataHelper.CreateGuildUser();
        var fromUser = DataHelper.CreateGuildUser("User2", 123456, "9513", "Test");
        var guild = DataHelper.CreateGuild();

        var profile = new UnverifyUserProfile(guildUser, DateTime.MinValue, DateTime.MaxValue, false);
        var logItem = await Service.LogUnverifyAsync(profile, guild, fromUser, CancellationToken.None);

        Assert.IsNotNull(logItem);
        Assert.IsTrue(logItem.Id > 0);
    }

    [TestMethod]
    public async Task LogSelfUnverifyAsync()
    {
        var guildUser = DataHelper.CreateGuildUser();
        var guild = DataHelper.CreateGuild();

        var profile = new UnverifyUserProfile(guildUser, DateTime.MinValue, DateTime.MaxValue, false);
        var logItem = await Service.LogSelfunverifyAsync(profile, guild, CancellationToken.None);

        Assert.IsNotNull(logItem);
        Assert.IsTrue(logItem.Id > 0);
    }

    [TestMethod]
    public async Task LogRemoveAsync()
    {
        var returnedRoles = new List<IRole>() { DataHelper.CreateRole() };
        var returnedChannels = new List<ChannelOverride>();
        var toUser = DataHelper.CreateGuildUser();
        var guild = DataHelper.CreateGuild();
        var fromUser = DataHelper.CreateGuildUser("User2", 123456, "9513", "Test");

        await Service.LogRemoveAsync(returnedRoles, returnedChannels, guild, fromUser, toUser, CancellationToken.None);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task LogUpdateAsync()
    {
        var toUser = DataHelper.CreateGuildUser();
        var guild = DataHelper.CreateGuild();
        var fromUser = DataHelper.CreateGuildUser("User2", 123456, "9513", "Test");

        await Service.LogUpdateAsync(DateTime.MinValue, DateTime.MaxValue, guild, fromUser, toUser, CancellationToken.None);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task LogRecoverAsync()
    {
        var returnedRoles = new List<IRole>() { DataHelper.CreateRole() };
        var returnedChannels = new List<ChannelOverride>();
        var toUser = DataHelper.CreateGuildUser();
        var guild = DataHelper.CreateGuild();
        var fromUser = DataHelper.CreateGuildUser("User2", 123456, "9513", "Test");

        await Service.LogRecoverAsync(returnedRoles, returnedChannels, guild, fromUser, toUser, CancellationToken.None);
        Assert.IsTrue(true);
    }
}