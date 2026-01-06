using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Tests.Helpers;
using Xunit;

namespace TodoApi.Tests.Integration;

public class TodoControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext? _context;
    private readonly int _testUserId = 1;

    public TodoControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        ApplicationDbContext? context = null;
        
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                // Replace authentication with test handler
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Build service provider to get context
                var serviceProvider = services.BuildServiceProvider();
                context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            });
        });

        _context = context;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetTodos_ReturnsOk_WithUserTodos()
    {
        // Arrange
        var todo1 = new Todo { UserId = _testUserId, Title = "Todo 1", CreatedAt = DateTime.UtcNow };
        var todo2 = new Todo { UserId = _testUserId, Title = "Todo 2", CreatedAt = DateTime.UtcNow };
        _context!.Todos.AddRange(todo1, todo2);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/todo");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoDto>>();
        Assert.NotNull(todos);
        Assert.Equal(2, todos.Count);
    }

    [Fact]
    public async Task GetTodos_ReturnsUnauthorized_WithoutAuth()
    {
        // Arrange
        var clientWithoutAuth = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Don't add test auth handler
            });
        }).CreateClient();

        // Act
        var response = await clientWithoutAuth.GetAsync("/api/todo");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTodo_ReturnsOk_WhenTodoExists()
    {
        // Arrange
        var todo = new Todo { UserId = _testUserId, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _context!.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/todo/{todo.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(result);
        Assert.Equal(todo.Id, result.Id);
        Assert.Equal("Test Todo", result.Title);
    }

    [Fact]
    public async Task GetTodo_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todo/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_ReturnsCreated_WithValidData()
    {
        // Arrange
        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            Description = "New Description",
            Priority = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todo", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("New Todo", result.Title);
        Assert.Equal("New Description", result.Description);
    }

    [Fact]
    public async Task CreateTodo_ReturnsBadRequest_WithEmptyTitle()
    {
        // Arrange
        var request = new CreateTodoRequest
        {
            Title = "",
            Description = "Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todo", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodo_ReturnsOk_WhenTodoExists()
    {
        // Arrange
        var todo = new Todo { UserId = _testUserId, Title = "Original", CreatedAt = DateTime.UtcNow };
        _context!.Todos.Add(todo);
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest
        {
            Title = "Updated Title",
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todo/{todo.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task UpdateTodo_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var request = new UpdateTodoRequest { Title = "Updated" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/todo/999", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_ReturnsNoContent_WhenTodoExists()
    {
        // Arrange
        var todo = new Todo { UserId = _testUserId, Title = "To Delete", CreatedAt = DateTime.UtcNow };
        _context!.Todos.Add(todo);
        await _context.SaveChangesAsync();
        var todoId = todo.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/todo/{todoId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify todo is deleted
        var deletedTodo = await _context.Todos.FindAsync(todoId);
        Assert.Null(deletedTodo);
    }

    [Fact]
    public async Task DeleteTodo_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todo/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_ReturnsOnlyUserTodos()
    {
        // Arrange
        var otherUserId = 2;
        var userTodo = new Todo { UserId = _testUserId, Title = "My Todo", CreatedAt = DateTime.UtcNow };
        var otherUserTodo = new Todo { UserId = otherUserId, Title = "Other Todo", CreatedAt = DateTime.UtcNow };
        _context!.Todos.AddRange(userTodo, otherUserTodo);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/todo");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoDto>>();
        Assert.NotNull(todos);
        Assert.Single(todos);
        Assert.Equal("My Todo", todos[0].Title);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _client?.Dispose();
    }
}

