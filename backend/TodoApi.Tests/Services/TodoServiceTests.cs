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

    [Fact]
    public async Task CreateTodoAsync_AssignsCategoriesSuccessfully()
    {
        // Arrange
        var userId = 1;
        var category1 = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        var category2 = new Category { Name = "Personal", Color = "#10B981", CreatedAt = DateTime.UtcNow };
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            CategoryIds = new List<int> { category1.Id, category2.Id }
        };

        // Act
        var result = await _service.CreateTodoAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Categories.Count);
        Assert.Contains(result.Categories, c => c.Id == category1.Id);
        Assert.Contains(result.Categories, c => c.Id == category2.Id);
    }

    [Fact]
    public async Task CreateTodoAsync_AssignsTagsSuccessfully()
    {
        // Arrange
        var userId = 1;
        var tag1 = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Important", Color = "#F59E0B", CreatedAt = DateTime.UtcNow };
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            TagIds = new List<int> { tag1.Id, tag2.Id }
        };

        // Act
        var result = await _service.CreateTodoAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains(result.Tags, t => t.Id == tag1.Id);
        Assert.Contains(result.Tags, t => t.Id == tag2.Id);
    }

    [Fact]
    public async Task CreateTodoAsync_IgnoresInvalidCategoryIds()
    {
        // Arrange
        var userId = 1;
        var validCategory = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(validCategory);
        await _context.SaveChangesAsync();

        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            CategoryIds = new List<int> { validCategory.Id, 999 } // 999 doesn't exist
        };

        // Act
        var result = await _service.CreateTodoAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Categories);
        Assert.Equal(validCategory.Id, result.Categories.First().Id);
    }

    [Fact]
    public async Task CreateTodoAsync_IgnoresInvalidTagIds()
    {
        // Arrange
        var userId = 1;
        var validTag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Tags.Add(validTag);
        await _context.SaveChangesAsync();

        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            TagIds = new List<int> { validTag.Id, 999 } // 999 doesn't exist
        };

        // Act
        var result = await _service.CreateTodoAsync(request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tags);
        Assert.Equal(validTag.Id, result.Tags.First().Id);
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_IncludesCategoriesAndTags()
    {
        // Arrange
        var userId = 1;
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(category);
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var todo = new Todo { UserId = userId, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        todo.TodoCategories.Add(new TodoCategory { TodoId = todo.Id, CategoryId = category.Id });
        todo.TodoTags.Add(new TodoTag { TodoId = todo.Id, TagId = tag.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTodosByUserIdAsync(userId)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Single(result[0].Categories);
        Assert.Single(result[0].Tags);
        Assert.Equal(category.Id, result[0].Categories.First().Id);
        Assert.Equal(tag.Id, result[0].Tags.First().Id);
    }

    [Fact]
    public async Task GetTodoByIdAsync_IncludesCategoriesAndTags()
    {
        // Arrange
        var userId = 1;
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(category);
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var todo = new Todo { UserId = userId, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        todo.TodoCategories.Add(new TodoCategory { TodoId = todo.Id, CategoryId = category.Id });
        todo.TodoTags.Add(new TodoTag { TodoId = todo.Id, TagId = tag.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTodoByIdAsync(todo.Id, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Categories);
        Assert.Single(result.Tags);
        Assert.Equal(category.Id, result.Categories.First().Id);
        Assert.Equal(tag.Id, result.Tags.First().Id);
    }

    [Fact]
    public async Task UpdateTodoAsync_UpdatesCategoriesSuccessfully()
    {
        // Arrange
        var userId = 1;
        var category1 = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        var category2 = new Category { Name = "Personal", Color = "#10B981", CreatedAt = DateTime.UtcNow };
        var category3 = new Category { Name = "Shopping", Color = "#F59E0B", CreatedAt = DateTime.UtcNow };
        _context.Categories.AddRange(category1, category2, category3);
        await _context.SaveChangesAsync();

        var todo = new Todo { UserId = userId, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        todo.TodoCategories.Add(new TodoCategory { TodoId = todo.Id, CategoryId = category1.Id });
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest
        {
            CategoryIds = new List<int> { category2.Id, category3.Id }
        };

        // Act
        var result = await _service.UpdateTodoAsync(todo.Id, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Categories.Count);
        Assert.Contains(result.Categories, c => c.Id == category2.Id);
        Assert.Contains(result.Categories, c => c.Id == category3.Id);
        Assert.DoesNotContain(result.Categories, c => c.Id == category1.Id);
    }

    [Fact]
    public async Task UpdateTodoAsync_UpdatesTagsSuccessfully()
    {
        // Arrange
        var userId = 1;
        var tag1 = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Important", Color = "#F59E0B", CreatedAt = DateTime.UtcNow };
        var tag3 = new Tag { Name = "Review", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Tags.AddRange(tag1, tag2, tag3);
        await _context.SaveChangesAsync();

        var todo = new Todo { UserId = userId, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        todo.TodoTags.Add(new TodoTag { TodoId = todo.Id, TagId = tag1.Id });
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest
        {
            TagIds = new List<int> { tag2.Id, tag3.Id }
        };

        // Act
        var result = await _service.UpdateTodoAsync(todo.Id, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains(result.Tags, t => t.Id == tag2.Id);
        Assert.Contains(result.Tags, t => t.Id == tag3.Id);
        Assert.DoesNotContain(result.Tags, t => t.Id == tag1.Id);
    }

    [Fact]
    public async Task UpdateTodoAsync_RemovesAllCategories_WhenEmptyListProvided()
    {
        // Arrange
        var userId = 1;
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var todo = new Todo { UserId = userId, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        todo.TodoCategories.Add(new TodoCategory { TodoId = todo.Id, CategoryId = category.Id });
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest
        {
            CategoryIds = new List<int>()
        };

        // Act
        var result = await _service.UpdateTodoAsync(todo.Id, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Categories);
    }

    [Fact]
    public async Task UpdateTodoAsync_RemovesAllTags_WhenEmptyListProvided()
    {
        // Arrange
        var userId = 1;
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var todo = new Todo { UserId = userId, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        todo.TodoTags.Add(new TodoTag { TodoId = todo.Id, TagId = tag.Id });
        await _context.SaveChangesAsync();

        var request = new UpdateTodoRequest
        {
            TagIds = new List<int>()
        };

        // Act
        var result = await _service.UpdateTodoAsync(todo.Id, request, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task GetTodosByCategoryAsync_ReturnsOnlyTodosWithCategory()
    {
        // Arrange
        var userId = 1;
        var category1 = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        var category2 = new Category { Name = "Personal", Color = "#10B981", CreatedAt = DateTime.UtcNow };
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        var todo1 = new Todo { UserId = userId, Title = "Work Todo", CreatedAt = DateTime.UtcNow };
        var todo2 = new Todo { UserId = userId, Title = "Personal Todo", CreatedAt = DateTime.UtcNow };
        var todo3 = new Todo { UserId = userId, Title = "Other Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.AddRange(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        todo1.TodoCategories.Add(new TodoCategory { TodoId = todo1.Id, CategoryId = category1.Id });
        todo2.TodoCategories.Add(new TodoCategory { TodoId = todo2.Id, CategoryId = category2.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTodosByCategoryAsync(userId, category1.Id)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Work Todo", result[0].Title);
        Assert.Contains(result[0].Categories, c => c.Id == category1.Id);
    }

    [Fact]
    public async Task GetTodosByTagAsync_ReturnsOnlyTodosWithTag()
    {
        // Arrange
        var userId = 1;
        var tag1 = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Important", Color = "#F59E0B", CreatedAt = DateTime.UtcNow };
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        var todo1 = new Todo { UserId = userId, Title = "Urgent Todo", CreatedAt = DateTime.UtcNow };
        var todo2 = new Todo { UserId = userId, Title = "Important Todo", CreatedAt = DateTime.UtcNow };
        var todo3 = new Todo { UserId = userId, Title = "Other Todo", CreatedAt = DateTime.UtcNow };
        _context.Todos.AddRange(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        todo1.TodoTags.Add(new TodoTag { TodoId = todo1.Id, TagId = tag1.Id });
        todo2.TodoTags.Add(new TodoTag { TodoId = todo2.Id, TagId = tag2.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetTodosByTagAsync(userId, tag1.Id)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Urgent Todo", result[0].Title);
        Assert.Contains(result[0].Tags, t => t.Id == tag1.Id);
    }

    [Fact]
    public async Task GetTodosByCategoryAsync_ReturnsEmptyList_WhenNoTodosHaveCategory()
    {
        // Arrange
        var userId = 1;
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTodosByCategoryAsync(userId, category.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTodosByTagAsync_ReturnsEmptyList_WhenNoTodosHaveTag()
    {
        // Arrange
        var userId = 1;
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTodosByTagAsync(userId, tag.Id);

        // Assert
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

