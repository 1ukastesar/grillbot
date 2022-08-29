﻿using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.App.Services;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services;

[TestClass]
public class DiscordExceptionHandlerTests : ServiceTest<DiscordExceptionHandler>
{
    private static IConfiguration Configuration => TestServices.Configuration.Value;

    private ITextChannel Channel { get; set; }
    private IUser User { get; set; }

    protected override DiscordExceptionHandler CreateService()
    {
        Channel = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId, Consts.ChannelName)
            .Build();

        var guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .SetGetTextChannelAction(Channel)
            .Build();

        User = new SelfUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .Build();

        var client = new ClientBuilder()
            .SetGetGuildAction(guild)
            .SetSelfUser((ISelfUser)User)
            .Build();

        var fileStorage = new FileStorageMock(Configuration);
        var profilePictureManager = new ProfilePictureManager(CacheBuilder, TestServices.CounterManager.Value);

        return new DiscordExceptionHandler(client, Configuration, fileStorage, profilePictureManager);
    }

    public override void Cleanup()
    {
        if (File.Exists("LastErrorDateTest.txt"))
            File.Delete("LastErrorDateTest.txt");
    }

    [TestMethod]
    public async Task CanHandleAsync_NullException()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Critical, "");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_Disabled()
    {
        var oldValue = Configuration["Discord:Logging:Enabled"];
        Configuration["Discord:Logging:Enabled"] = "false";

        try
        {
            var result = await Service.CanHandleAsync(LogSeverity.Debug, "", new Exception());
            Assert.IsFalse(result);
        }
        finally
        {
            Configuration["Discord:Logging:Enabled"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_InvalidSeverity()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Debug, "", new Exception());
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_IgnoredExceptions()
    {
        var resourceUnavailable = new SocketException();
        ReflectionHelper.SetPrivateReadonlyPropertyValue(resourceUnavailable, "NativeErrorCode", 11);

        var cases = new Exception[]
        {
            new GatewayReconnectException(""),
            new("", new GatewayReconnectException("")),
            new("Server missed last heartbeat"),
            new TaskCanceledException("", new IOException("", new SocketException((int)SocketError.ConnectionAborted))),
            new HttpRequestException("", resourceUnavailable),
            new("", new WebSocketException()),
            new("", new WebSocketClosedException(0)),
            new TaskCanceledException()
        };

        foreach (var @case in cases)
        {
            var result = await Service.CanHandleAsync(LogSeverity.Error, "Gateway", @case);
            Assert.IsFalse(result);
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_UnknownGuild()
    {
        var oldValue = Configuration["Discord:Logging:GuildId"];
        Configuration["Discord:Logging:GuildId"] = (Consts.GuildId + 1).ToString();

        try
        {
            ReflectionHelper.SetPrivateReadonlyPropertyValue(Service, "LogChannel", null);
            var result = await Service.CanHandleAsync(LogSeverity.Critical, "", new Exception());
            Assert.IsFalse(result);
        }
        finally
        {
            Configuration["Discord:Logging:GuildId"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_UnknownChannel()
    {
        var oldValue = Configuration["Discord:Logging:ChannelId"];
        Configuration["Discord:Logging:ChannelId"] = (Consts.ChannelId + 1).ToString();

        try
        {
            ReflectionHelper.SetPrivateReadonlyPropertyValue(Service, "LogChannel", null);
            var result = await Service.CanHandleAsync(LogSeverity.Critical, "", new Exception());
            Assert.IsFalse(result);
        }
        finally
        {
            Configuration["Discord:Logging:ChannelId"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Critical, "Test", new ArgumentException());
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task InfoAsync()
    {
        await Service.InfoAsync("Test", "Test");
    }

    [TestMethod]
    public async Task WarningAsync()
    {
        const string source = "Test";
        const string message = "Test";
        var exception = new ArgumentException();

        if (!await Service.CanHandleAsync(LogSeverity.Critical, source, exception))
            Assert.Fail();

        await Service.WarningAsync(source, message, exception);
    }
}