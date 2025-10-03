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
public sealed class TodoController : ControllerBase
{
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
        _logger.LogInformation("Retrieving todo items");
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
            _logger.LogWarning("Received null todo request");
            return BadRequest(new { error = "Request body required" });
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            _logger.LogWarning("Rejected todo item with empty title");
            return BadRequest(new { error = "Title is required" });
        }

        if (string.Equals(request.Title, "panic", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogCritical("Received todo item that triggers a critical scenario: {Title}", request.Title);
        }

        try
        {
            var item = _repository.Add(request.Title.Trim());
            _metrics.RecordCreated();
            _logger.LogInformation("Created todo item {Id}", item.Id);
            return Created($"/todo/{item.Id}", item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create todo item");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Could not create todo item" });
        }
    }
}
