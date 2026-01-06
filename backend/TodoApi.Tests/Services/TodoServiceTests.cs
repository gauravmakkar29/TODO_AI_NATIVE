using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Services;

public class TodoServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TodoService(_context);
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_ReturnsOnlyUserTodos()
    {
        // Arrange
        var userId1 = 1;
        var userId2 = 2;

        var user1Todo1 = new Todo { UserId = userId1, Title = "User1 Todo 1", CreatedAt = DateTime.UtcNow };
        var user1Todo2 = new Todo { UserId = userId1, Title = "User1 Todo 2", CreatedAt = DateTime.UtcNow };
        var user2Todo1 = new Todo { UserId = userId2, Title = "User2 Todo 1", CreatedAt = DateTime.UtcNow };

        _context.Todos.AddRange(user1Todo1, user1Todo2, user2Todo1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTodosByUserIdAsync(userId1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, todo => Assert.Equal(userId1, todo.Id == user1Todo1.Id || todo.Id == user1Todo2.Id ? userId1 : userId2));
        Assert.All(result, todo => Assert.Contains("User1", todo.Title));
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_ReturnsEmptyList_WhenNoTodosExist()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _service.GetTodosByUserIdAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var userId = 1;
        var todo1 = new Todo { UserId = userId, Title = "First", CreatedAt = DateTime.UtcNow.AddHours(-2) };
        var todo2 = new Todo { UserId = userId, Title = "Second", CreatedAt = DateTime.UtcNow.AddHours(-1) };
        var todo3 = new Todo { UserId = userId, Title = "Third", CreatedAt = DateTime.UtcNow };

        _context.Todos.AddRange(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTodosByUserIdAsync(userId)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Third", result[0].Title);
        Assert.Equal("Second", result[1].Title);
        Assert.Equal("First", result[2].Title);
    }

    [Fact]
    public async Task GetTodoByIdAsync_ReturnsTodo_WhenExists()
    {
        // Arrange
        var userId = 1;
        var todo = new Todo
        {
            UserId = userId,
            Title = "Test Todo",
            Description = "Test Description",
            IsCompleted = false,
            Priority = 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTodoByIdAsync(todo.Id, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todo.Id, result.Id);
        Assert.Equal("Test Todo", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.False(result.IsCompleted);
        Assert.Equal(1, result.Priority);
    }

    [Fact]
    public async Task GetTodoByIdAsync_ReturnsNull_WhenTodoDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var nonExistentId = 999;

        // Act
        var result = await _service.GetTodoByIdAsync(nonExistentId, userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTodoByIdAsync_ReturnsNull_WhenTodoBelongsToDifferentUser()
    {
        // Arrange
        var userId1 = 1;
        var userId2 = 2;
        var todo = new Todo { UserId = userId1, Title = "User1 Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTodoByIdAsync(todo.Id, userId2);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTodoAsync_CreatesTodoSuccessfully()
    {
        // Arrange
        var userId = 1;
        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            Description = "New Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = 2
        };

        // Act
        var result = await _service.CreateTodoAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("New Todo", result.Title);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(2, result.Priority);
        Assert.False(result.IsCompleted);
        Assert.NotEqual(default(DateTime), result.CreatedAt);

        var savedTodo = await _context.Todos.FindAsync(result.Id);
        Assert.NotNull(savedTodo);
        Assert.Equal(userId, savedTodo.UserId);
    }

    [Fact]
    public async Task CreateTodoAsync_SetsDefaultValues()
    {
        // Arrange
        var userId = 1;
        var request = new CreateTodoRequest
        {
            Title = "Simple Todo"
        };

        // Act
        var result = await _service.CreateTodoAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsCompleted);
        Assert.Equal(0, result.Priority);
        Assert.Null(result.Description);
        Assert.Null(result.DueDate);
    }

    [Fact]
    public async Task UpdateTodoAsync_UpdatesTodoSuccessfully()
    {
        // Arrange
        var userId = 1;
        var todo = new Todo
        {
            UserId = userId,
            Title = "Original Title",
            Description = "Original Description",
            IsCompleted = false,
            Priority = 0,
            CreatedAt = DateTime.UtcNow
        };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            IsCompleted = true,
            Priority = 2
        };

        // Act
        var result = await _service.UpdateTodoAsync(todo.Id, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.True(result.IsCompleted);
        Assert.Equal(2, result.Priority);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTodoAsync_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var userId = 1;
        var todo = new Todo
        {
            UserId = userId,
            Title = "Original Title",
            Description = "Original Description",
            IsCompleted = false,
            Priority = 0,
            CreatedAt = DateTime.UtcNow
        };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest
        {
            IsCompleted = true
        };

        // Act
        var result = await _service.UpdateTodoAsync(todo.Id, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Original Title", result.Title);
        Assert.Equal("Original Description", result.Description);
        Assert.True(result.IsCompleted);
        Assert.Equal(0, result.Priority);
    }

    [Fact]
    public async Task UpdateTodoAsync_ReturnsNull_WhenTodoDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var nonExistentId = 999;
        var request = new UpdateTodoRequest { Title = "Updated" };

        // Act
        var result = await _service.UpdateTodoAsync(nonExistentId, request, userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTodoAsync_ReturnsNull_WhenTodoBelongsToDifferentUser()
    {
        // Arrange
        var userId1 = 1;
        var userId2 = 2;
        var todo = new Todo { UserId = userId1, Title = "User1 Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest { Title = "Updated" };

        // Act
        var result = await _service.UpdateTodoAsync(todo.Id, request, userId2);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTodoAsync_DeletesTodoSuccessfully()
    {
        // Arrange
        var userId = 1;
        var todo = new Todo { UserId = userId, Title = "To Delete", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        var todoId = todo.Id;

        // Act
        var result = await _service.DeleteTodoAsync(todoId, userId);

        // Assert
        Assert.True(result);
        var deletedTodo = await _context.Todos.FindAsync(todoId);
        Assert.Null(deletedTodo);
    }

    [Fact]
    public async Task DeleteTodoAsync_ReturnsFalse_WhenTodoDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var nonExistentId = 999;

        // Act
        var result = await _service.DeleteTodoAsync(nonExistentId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteTodoAsync_ReturnsFalse_WhenTodoBelongsToDifferentUser()
    {
        // Arrange
        var userId1 = 1;
        var userId2 = 2;
        var todo = new Todo { UserId = userId1, Title = "User1 Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteTodoAsync(todo.Id, userId2);

        // Assert
        Assert.False(result);
        var stillExists = await _context.Todos.FindAsync(todo.Id);
        Assert.NotNull(stillExists);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

