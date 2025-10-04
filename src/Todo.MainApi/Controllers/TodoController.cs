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
    private static readonly Action<ILogger, Exception?> FetchingTodosFromDownstream = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(FetchingTodosFromDownstream)),
        "Fetching todos from downstream");

    private static readonly Action<ILogger, int, Exception?> FetchedTodosCount = LoggerMessage.Define<int>(
        LogLevel.Debug,
        new EventId(2, nameof(FetchedTodosCount)),
        "Fetched {Count} todos");

    private static readonly Action<ILogger, Exception?> ForwardingTodoCreation = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(3, nameof(ForwardingTodoCreation)),
        "Forwarding todo creation to downstream");

    private static readonly Action<ILogger, int, Exception?> DownstreamCreatedTodo = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(4, nameof(DownstreamCreatedTodo)),
        "Downstream created todo {Id}");

    private static readonly Action<ILogger, Exception?> DownstreamCallFailed = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(5, nameof(DownstreamCallFailed)),
        "Downstream call failed");

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
        FetchingTodosFromDownstream(_logger, null);
        var todos = await _client.GetAllAsync(cancellationToken).ConfigureAwait(false);
        FetchedTodosCount(_logger, todos.Count, null);
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

        ForwardingTodoCreation(_logger, null);

        try
        {
            var item = await _client.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            DownstreamCreatedTodo(_logger, item.Id, null);
            return Created(new Uri($"/todo/{item.Id}", UriKind.Relative), item);
        }
        catch (HttpRequestException ex)
        {
            DownstreamCallFailed(_logger, ex);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Downstream API unavailable" });
        }
    }
}
