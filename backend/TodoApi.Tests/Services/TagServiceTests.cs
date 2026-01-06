using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Services;

public class TagServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TagService _service;

    public TagServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TagService(_context);
    }

    [Fact]
    public async Task GetAllTagsAsync_ReturnsAllTags()
    {
        // Arrange
        var tag1 = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Important", Color = "#F59E0B", CreatedAt = DateTime.UtcNow };
        _context.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllTagsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllTagsAsync_ReturnsEmptyList_WhenNoTagsExist()
    {
        // Act
        var result = await _service.GetAllTagsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTagByIdAsync_ReturnsTag_WhenExists()
    {
        // Arrange
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTagByIdAsync(tag.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tag.Id, result.Id);
        Assert.Equal("Urgent", result.Name);
        Assert.Equal("#EF4444", result.Color);
    }

    [Fact]
    public async Task GetTagByIdAsync_ReturnsNull_WhenTagDoesNotExist()
    {
        // Act
        var result = await _service.GetTagByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTagAsync_CreatesTagSuccessfully()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "Urgent",
            Color = "#EF4444",
            Description = "Urgent tasks"
        };

        // Act
        var result = await _service.CreateTagAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Urgent", result.Name);
        Assert.Equal("#EF4444", result.Color);
        Assert.Equal("Urgent tasks", result.Description);

        var savedTag = await _context.Tags.FindAsync(result.Id);
        Assert.NotNull(savedTag);
    }

    [Fact]
    public async Task CreateTagAsync_ThrowsException_WhenTagNameExists()
    {
        // Arrange
        var existingTag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Tags.Add(existingTag);
        await _context.SaveChangesAsync();

        var request = new CreateTagRequest
        {
            Name = "Urgent",
            Color = "#10B981"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateTagAsync(request));
    }

    [Fact]
    public async Task UpdateTagAsync_UpdatesTagSuccessfully()
    {
        // Arrange
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var request = new UpdateTagRequest
        {
            Name = "Very Urgent",
            Color = "#DC2626",
            Description = "Updated description"
        };

        // Act
        var result = await _service.UpdateTagAsync(tag.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Very Urgent", result.Name);
        Assert.Equal("#DC2626", result.Color);
        Assert.Equal("Updated description", result.Description);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTagAsync_ReturnsNull_WhenTagDoesNotExist()
    {
        // Arrange
        var request = new UpdateTagRequest { Name = "Updated" };

        // Act
        var result = await _service.UpdateTagAsync(999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteTagAsync_DeletesTagSuccessfully()
    {
        // Arrange
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        var tagId = tag.Id;

        // Act
        var result = await _service.DeleteTagAsync(tagId);

        // Assert
        Assert.True(result);
        var deletedTag = await _context.Tags.FindAsync(tagId);
        Assert.Null(deletedTag);
    }

    [Fact]
    public async Task DeleteTagAsync_ReturnsFalse_WhenTagDoesNotExist()
    {
        // Act
        var result = await _service.DeleteTagAsync(999);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

