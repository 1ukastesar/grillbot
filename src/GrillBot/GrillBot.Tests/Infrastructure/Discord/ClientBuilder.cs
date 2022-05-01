﻿using Discord;
using Moq;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class ClientBuilder
{
    private Mock<IDiscordClient> Mock { get; }

    public ClientBuilder()
    {
        Mock = new Mock<IDiscordClient>();
    }

    public ClientBuilder SetSelfUser(SelfUserBuilder builder)
    {
        Mock.Setup(o => o.CurrentUser).Returns(builder.Build());
        return this;
    }

    public ClientBuilder SetGetApplicationAction(IApplication application)
    {
        Mock.Setup(o => o.GetApplicationInfoAsync(It.IsAny<RequestOptions>())).Returns(Task.FromResult(application));
        return this;
    }

    public ClientBuilder SetGetGuildAction(IGuild guild)
    {
        Mock.Setup(o => o.GetGuildAsync(It.Is<ulong>(x => x == guild.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild));
        return this;
    }

    public ClientBuilder SetGetGuildsAction(IEnumerable<IGuild> guilds)
    {
        Mock.Setup(o => o.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guilds.ToList().AsReadOnly() as IReadOnlyCollection<IGuild>));
        return this;
    }

    public ClientBuilder SetGetUserAction(IUser user)
    {
        Mock.Setup(o => o.GetUserAsync(It.Is<ulong>(x => x == user.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(user));
        return this;
    }

    public IDiscordClient Build() => Mock.Object;
}
