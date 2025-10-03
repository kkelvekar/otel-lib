using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Todo.MainApi.Models;

namespace Todo.MainApi.Services;

public sealed class DownstreamTodoClient : IDownstreamTodoClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DownstreamTodoClient> _logger;

    public DownstreamTodoClient(HttpClient httpClient, ILogger<DownstreamTodoClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Issuing GET /todo to downstream");
        using var response = await _httpClient.GetAsync("/todo", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(cancellationToken: cancellationToken).ConfigureAwait(false)
            ?? new List<TodoItem>();
        return todos;
    }

    public async Task<TodoItem> CreateAsync(TodoItemRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("Issuing POST /todo to downstream");
        using var response = await _httpClient.PostAsJsonAsync("/todo", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError("Downstream rejected request: {StatusCode} {Body}", response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        var todo = await response.Content.ReadFromJsonAsync<TodoItem>(cancellationToken: cancellationToken).ConfigureAwait(false);
        return todo ?? throw new InvalidOperationException("Downstream response missing body");
    }
}
