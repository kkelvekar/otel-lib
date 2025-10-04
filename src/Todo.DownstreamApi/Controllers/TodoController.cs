using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Todo.DownstreamApi.Models;
using Todo.DownstreamApi.Repositories;
using Todo.DownstreamApi.Services;

namespace Todo.DownstreamApi.Controllers;

[ApiController]
[Route("todo")]
internal sealed class TodoController : ControllerBase
{
    private static readonly Action<ILogger, Exception?> RetrievingTodoItems = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(RetrievingTodoItems)),
        "Retrieving todo items");

    private static readonly Action<ILogger, Exception?> NullTodoRequest = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(2, nameof(NullTodoRequest)),
        "Received null todo request");

    private static readonly Action<ILogger, Exception?> EmptyTitleRejected = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(3, nameof(EmptyTitleRejected)),
        "Rejected todo item with empty title");

    private static readonly Action<ILogger, string, Exception?> CriticalTodoTriggered = LoggerMessage.Define<string>(
        LogLevel.Critical,
        new EventId(4, nameof(CriticalTodoTriggered)),
        "Received todo item that triggers a critical scenario: {Title}");

    private static readonly Action<ILogger, int, Exception?> TodoItemCreated = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(5, nameof(TodoItemCreated)),
        "Created todo item {Id}");

    private static readonly Action<ILogger, Exception?> TodoItemCreationFailed = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(6, nameof(TodoItemCreationFailed)),
        "Failed to create todo item");

    private readonly ITodoRepository _repository;
    private readonly TodoMetrics _metrics;
    private readonly ILogger<TodoController> _logger;

    public TodoController(ITodoRepository repository, TodoMetrics metrics, ILogger<TodoController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<TodoItem>> Get()
    {
        RetrievingTodoItems(_logger, null);
        var stopwatch = Stopwatch.StartNew();
        var items = _repository.GetAll().ToArray();
        stopwatch.Stop();

        _metrics.RecordRead(stopwatch.Elapsed, items.Length);
        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<TodoItem> Post([FromBody] TodoItemRequest request)
    {
        if (request is null)
        {
            NullTodoRequest(_logger, null);
            return BadRequest(new { error = "Request body required" });
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            EmptyTitleRejected(_logger, null);
            return BadRequest(new { error = "Title is required" });
        }

        if (string.Equals(request.Title, "panic", StringComparison.OrdinalIgnoreCase))
        {
            CriticalTodoTriggered(_logger, request.Title, null);
        }

        try
        {
            var item = _repository.Add(request.Title.Trim());
            _metrics.RecordCreated();
            TodoItemCreated(_logger, item.Id, null);
            return Created(new Uri($"/todo/{item.Id}", UriKind.Relative), item);
        }
        catch (InvalidOperationException ex)
        {
            TodoItemCreationFailed(_logger, ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Could not create todo item" });
        }
    }
}
