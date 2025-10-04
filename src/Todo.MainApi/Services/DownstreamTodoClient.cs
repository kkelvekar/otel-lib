using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Todo.MainApi.Models;

namespace Todo.MainApi.Services;

public sealed class DownstreamTodoClient : IDownstreamTodoClient
{
    private static readonly Action<ILogger, Exception?> IssuingGetTodo = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(IssuingGetTodo)),
        "Issuing GET /todo to downstream");

    private static readonly Action<ILogger, Exception?> IssuingPostTodo = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2, nameof(IssuingPostTodo)),
        "Issuing POST /todo to downstream");

    private static readonly Action<ILogger, HttpStatusCode, string, Exception?> DownstreamRejectedRequest = LoggerMessage.Define<HttpStatusCode, string>(
        LogLevel.Error,
        new EventId(3, nameof(DownstreamRejectedRequest)),
        "Downstream rejected request: {StatusCode} {Body}");

    private readonly HttpClient _httpClient;
    private readonly ILogger<DownstreamTodoClient> _logger;

    public DownstreamTodoClient(HttpClient httpClient, ILogger<DownstreamTodoClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IssuingGetTodo(_logger, null);
        using var response = await _httpClient.GetAsync(new Uri("/todo", UriKind.Relative), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(cancellationToken: cancellationToken).ConfigureAwait(false)
            ?? new List<TodoItem>();
        return todos;
    }

    public async Task<TodoItem> CreateAsync(TodoItemRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        IssuingPostTodo(_logger, null);
        using var response = await _httpClient.PostAsJsonAsync(new Uri("/todo", UriKind.Relative), request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            DownstreamRejectedRequest(_logger, response.StatusCode, body, null);
            response.EnsureSuccessStatusCode();
        }

        var todo = await response.Content.ReadFromJsonAsync<TodoItem>(cancellationToken: cancellationToken).ConfigureAwait(false);
        return todo ?? throw new InvalidOperationException("Downstream response missing body");
    }
}
