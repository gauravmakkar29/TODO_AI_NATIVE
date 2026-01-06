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

public class TagControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext? _context;

    public TagControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task GetTags_ReturnsOk_WithTags()
    {
        // Arrange
        var tag1 = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        var tag2 = new Tag { Name = "Important", Color = "#F59E0B", CreatedAt = DateTime.UtcNow };
        _context!.Tags.AddRange(tag1, tag2);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/tag");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tags = await response.Content.ReadFromJsonAsync<List<TagDto>>();
        Assert.NotNull(tags);
        Assert.Equal(2, tags.Count);
    }

    [Fact]
    public async Task GetTag_ReturnsOk_WhenTagExists()
    {
        // Arrange
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context!.Tags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/tag/{tag.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(result);
        Assert.Equal(tag.Id, result.Id);
        Assert.Equal("Urgent", result.Name);
    }

    [Fact]
    public async Task GetTag_ReturnsNotFound_WhenTagDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/tag/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTag_ReturnsCreated_WithValidData()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "Urgent",
            Color = "#EF4444",
            Description = "Urgent tasks"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tag", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Urgent", result.Name);
        Assert.Equal("#EF4444", result.Color);
    }

    [Fact]
    public async Task CreateTag_ReturnsBadRequest_WithEmptyName()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "",
            Color = "#EF4444"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tag", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTag_ReturnsOk_WhenTagExists()
    {
        // Arrange
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context!.Tags.Add(tag);
        await _context.SaveChangesAsync();

        var request = new UpdateTagRequest
        {
            Name = "Very Urgent",
            Color = "#DC2626"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tag/{tag.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(result);
        Assert.Equal("Very Urgent", result.Name);
        Assert.Equal("#DC2626", result.Color);
    }

    [Fact]
    public async Task DeleteTag_ReturnsNoContent_WhenTagExists()
    {
        // Arrange
        var tag = new Tag { Name = "Urgent", Color = "#EF4444", CreatedAt = DateTime.UtcNow };
        _context!.Tags.Add(tag);
        await _context.SaveChangesAsync();
        var tagId = tag.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/tag/{tagId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var deletedTag = await _context.Tags.FindAsync(tagId);
        Assert.Null(deletedTag);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _client?.Dispose();
    }
}

