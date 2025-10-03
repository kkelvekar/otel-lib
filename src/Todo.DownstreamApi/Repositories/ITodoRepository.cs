using System.Collections.Generic;
using Todo.DownstreamApi.Models;

namespace Todo.DownstreamApi.Repositories;

public interface ITodoRepository
{
    IReadOnlyCollection<TodoItem> GetAll();

    TodoItem Add(string title);
}
