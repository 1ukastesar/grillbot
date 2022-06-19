﻿using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using GrillBot.Data.Models.API.Help;
using Microsoft.AspNetCore.Http;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.Data.Exceptions;
using GrillBot.App.Services.User;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Common.Models;
using GrillBot.Data.Extensions;
using GrillBot.Database.Models;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/users")]
[OpenApiTag("Users", Description = "User management")]
public class UsersController : Controller
{
    private CommandsHelpService HelpService { get; }
    private ExternalCommandsHelpService ExternalCommandsHelpService { get; }
    private UsersApiService ApiService { get; }
    private RubbergodKarmaService KarmaService { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public UsersController(CommandsHelpService helpService, ExternalCommandsHelpService externalCommandsHelpService,
        UsersApiService apiService, RubbergodKarmaService karmaService, ApiRequestContext apiRequestContext)
    {
        HelpService = helpService;
        ExternalCommandsHelpService = externalCommandsHelpService;
        ApiService = apiService;
        KarmaService = karmaService;
        ApiRequestContext = apiRequestContext;
    }

    /// <summary>
    /// Get paginated list of users.
    /// </summary>
    /// <response code="200">Returns paginated list of users.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<UserListItem>>> GetUsersListAsync([FromQuery] GetUserListParams parameters)
    {
        parameters.FixStatus();

        var result = await ApiService.GetListAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about user.
    /// </summary>
    /// <response code="200">Returns detailed information about user.</response>
    /// <response code="404">User not found in database.</response>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetail>> GetUserDetailAsync(ulong id)
    {
        var result = await ApiService.GetUserDetailAsync(id);

        if (result == null)
            return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));

        return Ok(result);
    }

    /// <summary>
    /// Gets data about currently logged user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Only for users with User permissions.</remarks>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserDetail>> GetCurrentUserDetailAsync()
    {
        var loggedUserId = ApiRequestContext.GetUserId();
        var user = await GetUserDetailAsync(loggedUserId);

        switch (user.Result)
        {
            case NotFoundObjectResult:
                return user;
            // Remove private data. User not have permission to view this.
            case OkObjectResult { Value: UserDetail userDetail }:
                userDetail.RemoveSecretData();
                return user;
            default:
                throw new InvalidOperationException("Při načítání aktuálně přihlášeného uživatele došlo k neočekávanému výstupu.");
        }
    }

    /// <summary>
    /// Gets non paginated list of user available text commands.
    /// </summary>
    [HttpGet("me/commands")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableCommandsAsync()
    {
        var loggedUserId = ApiRequestContext.GetUserId();
        var result = await HelpService.GetHelpAsync(loggedUserId);
        return Ok(result);
    }

    /// <summary>
    /// Get non paginated list of available commands from external service.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="500">Something is wrong</response>
    [HttpGet("me/commands/{service}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableExternalCommandsAsync(string service)
    {
        try
        {
            var loggedUserId = ApiRequestContext.GetUserId();
            service = char.ToUpper(service[0]) + service[1..].ToLower();
            var result = await ExternalCommandsHelpService.GetHelpAsync(service, loggedUserId);
            return Ok(result);
        }
        catch (GrillBotException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new MessageResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update user.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> UpdateUserAsync(ulong id, [FromBody] UpdateUserParams parameters)
    {
        try
        {
            this.SetApiRequestData(parameters);
            await ApiService.UpdateUserAsync(id, parameters);
            return Ok();
        }
        catch (NotFoundException)
        {
            return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));
        }
    }

    /// <summary>
    /// Heartbeat event to set the user to be logged in to the administration.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpPost("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> HearthbeatAsync()
    {
        await ApiService.SetHearthbeatStatusAsync(true);
        return Ok();
    }

    /// <summary>
    /// Heartbeat event to set that the user is no longer logged in to the administration.
    /// </summary>
    [HttpDelete("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> HearthbeatOffAsync()
    {
        await ApiService.SetHearthbeatStatusAsync(false);
        return Ok();
    }

    /// <summary>
    /// Gets complete list of user points.
    /// </summary>
    /// <response code="200">Returns full points board.</response>
    [HttpGet("points/board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<UserPointsItem>>> GetPointsLeaderboardAsync()
    {
        var result = await ApiService.GetPointsBoardAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get rubbergod karma leaderboard.
    /// </summary>
    /// <response code="200">Returns paginated response of karma leaderboard</response>
    /// <response code="500">Something is wrong.</response>
    [ApiKeyAuth]
    [HttpGet("karma")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    [ResponseCache(CacheProfileName = "BoardApi", VaryByQueryKeys = new[] { "*" })]
    public async Task<ActionResult<PaginatedResponse<UserKarma>>> GetRubbergodUserKarmaAsync([FromQuery] KarmaListParams parameters)
    {
        try
        {
            var result = await KarmaService.GetUserKarmaAsync(parameters.Sort, parameters.Pagination);
            return Ok(result);
        }
        catch (GrillBotException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new MessageResponse(ex.Message));
        }
    }
}
