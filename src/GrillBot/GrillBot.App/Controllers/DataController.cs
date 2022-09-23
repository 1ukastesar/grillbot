﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/data")]
[ApiExplorerSettings(GroupName = "v1")]
public class DataController : Controller
{
    private EmotesCacheService EmotesCacheService { get; }
    private IMapper Mapper { get; }
    private IServiceProvider ServiceProvider { get; }

    public DataController(EmotesCacheService emotesCacheService, IMapper mapper, IServiceProvider serviceProvider)
    {
        EmotesCacheService = emotesCacheService;
        Mapper = mapper;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get non paginated list of available guilds.
    /// </summary>
    [HttpGet("guilds")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetAvailableGuildsAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetAvailableGuilds>();
        var result = await action.ProcessAsync();

        return Ok(result);
    }

    /// <summary>
    /// Get non paginated list of channels.
    /// </summary>
    /// <param name="guildId">Optional guild ID</param>
    /// <param name="ignoreThreads">Flag that removes threads from list.</param>
    [HttpGet("channels")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetChannelsAsync(ulong? guildId, bool ignoreThreads = false)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.GetChannelSimpleList>();
        var result = await action.ProcessAsync(guildId, ignoreThreads);

        return Ok(result);
    }

    /// <summary>
    /// Get roles
    /// </summary>
    [HttpGet("roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetRolesAsync(ulong? guildId)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetRoles>();
        var result = await action.ProcessAsync(guildId);

        return Ok(result);
    }

    /// <summary>
    /// Get non-paginated commands list
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("commands")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<List<string>> GetCommandsList()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.GetCommandsList>();
        var result = action.Process();

        return Ok(result);
    }

    /// <summary>
    /// Gets non-paginated list of users.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetAvailableUsersAsync(bool? bots = null)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.User.GetAvailableUsers>();
        var result = await action.ProcessAsync(bots);

        return Ok(result);
    }

    /// <summary>
    /// Get currently supported emotes.
    /// </summary>
    [HttpGet("emotes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<EmoteItem>> GetSupportedEmotes()
    {
        var emotes = EmotesCacheService.GetSupportedEmotes();

        var result = Mapper.Map<List<EmoteItem>>(emotes)
            .OrderBy(o => o.Name)
            .ToList();

        return Ok(result);
    }
}
