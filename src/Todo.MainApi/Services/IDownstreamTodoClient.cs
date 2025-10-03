using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Todo.MainApi.Models;

namespace Todo.MainApi.Services;

public interface IDownstreamTodoClient
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TodoItem> CreateAsync(TodoItemRequest request, CancellationToken cancellationToken = default);
}
