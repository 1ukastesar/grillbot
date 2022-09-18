﻿using System.Security.Claims;
using GrillBot.App.Actions;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Infrastructure.Common;

public abstract class ApiActionTest<TAction> : ActionTest<TAction> where TAction : ApiAction
{
    protected static readonly Lazy<ApiRequestContext> UserApiRequestContext
        = new(() => CreateApiRequestContext("User"), LazyThreadSafetyMode.ExecutionAndPublication);

    protected static readonly Lazy<ApiRequestContext> AdminApiRequestContext
        = new(() => CreateApiRequestContext("Admin"), LazyThreadSafetyMode.ExecutionAndPublication);
    
    protected ApiRequestContext ApiRequestContext { get; private set; }

    private static ApiRequestContext CreateApiRequestContext(string role)
    {
        return new ApiRequestContext
        {
            LoggedUser = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username + "-" + role, Consts.Discriminator).Build(),
            LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
            }))
        };
    }

    protected override void Init()
    {
        ApiRequestContext = IsPublic() ? UserApiRequestContext.Value : AdminApiRequestContext.Value;
    }
}