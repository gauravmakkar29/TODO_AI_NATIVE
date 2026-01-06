using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;

    public TagService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return tags;
    }

    public async Task<TagDto?> GetTagByIdAsync(int tagId)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            Description = tag.Description,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }

    public async Task<TagDto> CreateTagAsync(CreateTagRequest request)
    {
        // Check if tag with same name already exists
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower());

        if (existingTag != null)
            throw new InvalidOperationException($"Tag with name '{request.Name}' already exists.");

        var tag = new Tag
        {
            Name = request.Name,
            Color = request.Color,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            Description = tag.Description,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }

    public async Task<TagDto?> UpdateTagAsync(int tagId, UpdateTagRequest request)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            return null;

        // Check if new name conflicts with existing tag
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.ToLower() != tag.Name.ToLower())
        {
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower() && t.Id != tagId);

            if (existingTag != null)
                throw new InvalidOperationException($"Tag with name '{request.Name}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            tag.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Color))
            tag.Color = request.Color;

        if (request.Description != null)
            tag.Description = request.Description;

        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            Description = tag.Description,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }

    public async Task<bool> DeleteTagAsync(int tagId)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return true;
    }
}

