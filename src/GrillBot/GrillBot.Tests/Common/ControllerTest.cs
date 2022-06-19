﻿using GrillBot.Cache.Services.Repository;
using GrillBot.Data.Models.API;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using GrillBot.Common.Models;
using GrillBot.Database.Services.Repository;
using GrillBot.Tests.Infrastructure.Database;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ControllerTest<TController> where TController : Controller
{
    protected TController AdminController { get; private set; }
    protected TController UserController { get; private set; }

    protected ApiRequestContext UserApiRequestContext { get; private set; }
    protected ApiRequestContext AdminApiRequestContext { get; private set; }

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected IServiceProvider ServiceProvider { get; private set; }

    protected abstract bool CanInitProvider();
    protected abstract TController CreateController();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = new TestDatabaseBuilder();
        CacheBuilder = new TestCacheBuilder();
        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();
        UserApiRequestContext = CreateApiRequestContext("User");
        AdminApiRequestContext = CreateApiRequestContext("Admin");
        ServiceProvider = CreateProvider(CanInitProvider());

        AdminController = CreateController();
        AdminController.ControllerContext = CreateContext(AdminApiRequestContext);

        UserController = CreateController();
        UserController.ControllerContext = CreateContext(UserApiRequestContext);
    }

    public virtual void Cleanup()
    {
    }

    [TestCleanup]
    public void TestClean()
    {
        Cleanup();

        TestDatabaseBuilder.ClearDatabase();
        CacheBuilder.ClearDatabase();
        Repository.Dispose();
        CacheRepository.Dispose();
        UserController.Dispose();
        AdminController.Dispose();
    }

    private static ApiRequestContext CreateApiRequestContext(string role)
    {
        return new ApiRequestContext
        {
            LoggedUser = new UserBuilder().SetId(Consts.UserId).SetUsername(Consts.Username).Build(),
            LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
            }))
        };
    }

    private ControllerContext CreateContext(ApiRequestContext context)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = context.LoggedUserData!,
                RequestServices = ServiceProvider
            }
        };
    }

    protected void CheckResult<TResult>(IActionResult result) where TResult : IActionResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(TResult));

        switch (result)
        {
            case NotFoundObjectResult notFound:
                Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
                break;
            case FileContentResult fileContent:
                Assert.IsNotNull(fileContent.FileContents);
                Assert.IsTrue(fileContent.FileContents.Length > 0);
                break;
        }
    }

    protected void CheckResult<TResult, TOkModel>(ActionResult<TOkModel> result) where TResult : ObjectResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(TResult));

        switch (result.Result)
        {
            case OkObjectResult ok:
                Assert.IsInstanceOfType(ok.Value, typeof(TOkModel));
                break;
            case NotFoundObjectResult notFound:
                Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
                break;
            case BadRequestObjectResult badRequest:
                Assert.IsInstanceOfType(badRequest.Value, typeof(ValidationProblemDetails));
                break;
        }
    }

    private static IServiceProvider CreateProvider(bool init = false)
    {
        return init ? DiHelper.CreateInitializedProvider() : DiHelper.CreateEmptyProvider();
    }
}
