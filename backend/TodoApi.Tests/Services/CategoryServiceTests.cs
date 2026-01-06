using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Services;

public class CategoryServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new CategoryService(_context);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        // Arrange
        var category1 = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        var category2 = new Category { Name = "Personal", Color = "#10B981", CreatedAt = DateTime.UtcNow };
        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsEmptyList_WhenNoCategoriesExist()
    {
        // Act
        var result = await _service.GetAllCategoriesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsCategory_WhenExists()
    {
        // Arrange
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCategoryByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Work", result.Name);
        Assert.Equal("#3B82F6", result.Color);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
    {
        // Act
        var result = await _service.GetCategoryByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCategoryAsync_CreatesCategorySuccessfully()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Work",
            Color = "#3B82F6",
            Description = "Work tasks"
        };

        // Act
        var result = await _service.CreateCategoryAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Work", result.Name);
        Assert.Equal("#3B82F6", result.Color);
        Assert.Equal("Work tasks", result.Description);

        var savedCategory = await _context.Categories.FindAsync(result.Id);
        Assert.NotNull(savedCategory);
    }

    [Fact]
    public async Task CreateCategoryAsync_ThrowsException_WhenCategoryNameExists()
    {
        // Arrange
        var existingCategory = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(existingCategory);
        await _context.SaveChangesAsync();

        var request = new CreateCategoryRequest
        {
            Name = "Work",
            Color = "#10B981"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateCategoryAsync(request));
    }

    [Fact]
    public async Task UpdateCategoryAsync_UpdatesCategorySuccessfully()
    {
        // Arrange
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var request = new UpdateCategoryRequest
        {
            Name = "Updated Work",
            Color = "#10B981",
            Description = "Updated description"
        };

        // Act
        var result = await _service.UpdateCategoryAsync(category.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Work", result.Name);
        Assert.Equal("#10B981", result.Color);
        Assert.Equal("Updated description", result.Description);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ReturnsNull_WhenCategoryDoesNotExist()
    {
        // Arrange
        var request = new UpdateCategoryRequest { Name = "Updated" };

        // Act
        var result = await _service.UpdateCategoryAsync(999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCategoryAsync_DeletesCategorySuccessfully()
    {
        // Arrange
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        var categoryId = category.Id;

        // Act
        var result = await _service.DeleteCategoryAsync(categoryId);

        // Assert
        Assert.True(result);
        var deletedCategory = await _context.Categories.FindAsync(categoryId);
        Assert.Null(deletedCategory);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ReturnsFalse_WhenCategoryDoesNotExist()
    {
        // Act
        var result = await _service.DeleteCategoryAsync(999);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

