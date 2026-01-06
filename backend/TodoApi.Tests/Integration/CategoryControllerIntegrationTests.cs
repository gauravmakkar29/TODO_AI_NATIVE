using System.Net;
using System.Net.Http.Json;
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

public class CategoryControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext? _context;

    public CategoryControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        ApplicationDbContext? context = null;

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

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
    public async Task GetCategories_ReturnsOk_WithCategories()
    {
        // Arrange
        var category1 = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        var category2 = new Category { Name = "Personal", Color = "#10B981", CreatedAt = DateTime.UtcNow };
        _context!.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/category");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.NotNull(categories);
        Assert.Equal(2, categories.Count);
    }

    [Fact]
    public async Task GetCategory_ReturnsOk_WhenCategoryExists()
    {
        // Arrange
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context!.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/category/{category.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Work", result.Name);
    }

    [Fact]
    public async Task GetCategory_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/category/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreated_WithValidData()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Work",
            Color = "#3B82F6",
            Description = "Work tasks"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/category", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Work", result.Name);
        Assert.Equal("#3B82F6", result.Color);
    }

    [Fact]
    public async Task CreateCategory_ReturnsBadRequest_WithEmptyName()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "",
            Color = "#3B82F6"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/category", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsOk_WhenCategoryExists()
    {
        // Arrange
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context!.Categories.Add(category);
        await _context.SaveChangesAsync();

        var request = new UpdateCategoryRequest
        {
            Name = "Updated Work",
            Color = "#10B981"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/category/{category.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(result);
        Assert.Equal("Updated Work", result.Name);
        Assert.Equal("#10B981", result.Color);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNoContent_WhenCategoryExists()
    {
        // Arrange
        var category = new Category { Name = "Work", Color = "#3B82F6", CreatedAt = DateTime.UtcNow };
        _context!.Categories.Add(category);
        await _context.SaveChangesAsync();
        var categoryId = category.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/category/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var deletedCategory = await _context.Categories.FindAsync(categoryId);
        Assert.Null(deletedCategory);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _client?.Dispose();
    }
}

