using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Todo.MainApi.Models;
using Todo.MainApi.Services;

namespace Todo.MainApi.Controllers;

[ApiController]
[Route("todo")]
public sealed class TodoController : ControllerBase
{
    private readonly IDownstreamTodoClient _client;
    private readonly ILogger<TodoController> _logger;

    public TodoController(IDownstreamTodoClient client, ILogger<TodoController> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TodoItem>>> GetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching todos from downstream");
        var todos = await _client.GetAllAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Fetched {Count} todos", todos.Count);
        return Ok(todos);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TodoItem>> PostAsync([FromBody] TodoItemRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body required" });
        }

        _logger.LogInformation("Forwarding todo creation to downstream");

        try
        {
            var item = await _client.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Downstream created todo {Id}", item.Id);
            return Created($"/todo/{item.Id}", item);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Downstream call failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Downstream API unavailable" });
        }
    }
}
