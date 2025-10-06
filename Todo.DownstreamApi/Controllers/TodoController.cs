using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Todo.DownstreamApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private static readonly List<Todo> _todos = [];
        private readonly ILogger<TodoController> _logger;

        public TodoController(ILogger<TodoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Todo> Get()
        {
            _logger.LogInformation("Getting all todos");
            return _todos;
        }

        [HttpGet("{id}")]
        public ActionResult<Todo> Get(int id)
        {
            _logger.LogInformation("Getting todo with id {Id}", id);
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                _logger.LogWarning("Todo with id {Id} not found", id);
                return NotFound();
            }
            return todo;
        }

        [HttpPost]
        public ActionResult<Todo> Post([FromBody] Todo todo)
        {
            _logger.LogInformation("Creating a new todo");
            todo.Id = _todos.Count != 0 ? _todos.Max(t => t.Id) + 1 : 1;
            _todos.Add(todo);
            _logger.LogInformation("Created todo with id {Id}", todo.Id);
            return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Todo todo)
        {
            _logger.LogInformation("Updating todo with id {Id}", id);
            var existingTodo = _todos.FirstOrDefault(t => t.Id == id);
            if (existingTodo == null)
            {
                _logger.LogWarning("Todo with id {Id} not found", id);
                return NotFound();
            }
            existingTodo.Title = todo.Title;
            existingTodo.IsComplete = todo.IsComplete;
            _logger.LogInformation("Updated todo with id {Id}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Deleting todo with id {Id}", id);
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                _logger.LogWarning("Todo with id {Id} not found", id);
                return NotFound();
            }
            _todos.Remove(todo);
            _logger.LogInformation("Deleted todo with id {Id}", id);
            return NoContent();
        }
    }
}
