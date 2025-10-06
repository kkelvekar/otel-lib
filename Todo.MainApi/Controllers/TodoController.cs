using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Todo.MainApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TodoController> _logger;

        public TodoController(IHttpClientFactory httpClientFactory, ILogger<TodoController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DownstreamApi");
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<Todo>> Get()
        {
            _logger.LogInformation("Getting all todos from downstream API");
            var todos = await _httpClient.GetFromJsonAsync<IEnumerable<Todo>>("todo");
            return todos ?? [];
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> Get(int id)
        {
            _logger.LogInformation("Getting todo with id {Id} from downstream API", id);
            var todo = await _httpClient.GetFromJsonAsync<Todo>($"todo/{id}");
            if (todo == null)
            {
                _logger.LogWarning("Todo with id {Id} not found in downstream API", id);
                return NotFound();
            }
            return todo;
        }

        [HttpPost]
        public async Task<ActionResult<Todo>> Post([FromBody] Todo todo)
        {
            _logger.LogInformation("Creating a new todo via downstream API");
            var response = await _httpClient.PostAsJsonAsync("todo", todo);
            var createdTodo = await response.Content.ReadFromJsonAsync<Todo>();
            if (createdTodo == null)
            {
                _logger.LogError("Failed to create todo via downstream API");
                return BadRequest();
            }
            _logger.LogInformation("Created todo with id {Id} via downstream API", createdTodo.Id);
            return CreatedAtAction(nameof(Get), new { id = createdTodo.Id }, createdTodo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Todo todo)
        {
            _logger.LogInformation("Updating todo with id {Id} via downstream API", id);
            await _httpClient.PutAsJsonAsync($"todo/{id}", todo);
            _logger.LogInformation("Updated todo with id {Id} via downstream API", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting todo with id {Id} via downstream API", id);
            await _httpClient.DeleteAsync($"todo/{id}");
            _logger.LogInformation("Deleted todo with id {Id} via downstream API", id);
            return NoContent();
        }
    }
}
