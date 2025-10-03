using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Todo.DownstreamApi.Models;

namespace Todo.DownstreamApi.Repositories;

public sealed class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<int, TodoItem> _items = new();
    private int _nextId = 0;

    public IReadOnlyCollection<TodoItem> GetAll()
    {
        return _items.Values
            .OrderBy(item => item.Id)
            .ToArray();
    }

    public TodoItem Add(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        var id = Interlocked.Increment(ref _nextId);
        var todo = new TodoItem(id, title);
        if (!_items.TryAdd(id, todo))
        {
            throw new InvalidOperationException("Failed to store todo item");
        }

        return todo;
    }
}
