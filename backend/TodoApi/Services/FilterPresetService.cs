using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public class FilterPresetService : IFilterPresetService
{
    private readonly ApplicationDbContext _context;

    public FilterPresetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FilterPresetDto>> GetFilterPresetsByUserIdAsync(int userId)
    {
        var presets = await _context.FilterPresets
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return presets.Select(p => MapToDto(p));
    }

    public async Task<FilterPresetDto?> GetFilterPresetByIdAsync(int presetId, int userId)
    {
        var preset = await _context.FilterPresets
            .FirstOrDefaultAsync(p => p.Id == presetId && p.UserId == userId);

        if (preset == null)
            return null;

        return MapToDto(preset);
    }

    public async Task<FilterPresetDto> CreateFilterPresetAsync(CreateFilterPresetRequest request, int userId)
    {
        var preset = new FilterPreset
        {
            UserId = userId,
            Name = request.Name,
            SearchQuery = request.SearchQuery,
            IsCompleted = request.IsCompleted,
            IsOverdue = request.IsOverdue,
            Priority = request.Priority,
            CategoryIds = request.CategoryIds != null ? JsonSerializer.Serialize(request.CategoryIds) : null,
            TagIds = request.TagIds != null ? JsonSerializer.Serialize(request.TagIds) : null,
            DueDateFrom = request.DueDateFrom,
            DueDateTo = request.DueDateTo,
            CreatedAtFrom = request.CreatedAtFrom,
            CreatedAtTo = request.CreatedAtTo,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.FilterPresets.Add(preset);
        await _context.SaveChangesAsync();

        return MapToDto(preset);
    }

    public async Task<FilterPresetDto?> UpdateFilterPresetAsync(int presetId, UpdateFilterPresetRequest request, int userId)
    {
        var preset = await _context.FilterPresets
            .FirstOrDefaultAsync(p => p.Id == presetId && p.UserId == userId);

        if (preset == null)
            return null;

        if (!string.IsNullOrWhiteSpace(request.Name))
            preset.Name = request.Name;

        preset.SearchQuery = request.SearchQuery;
        preset.IsCompleted = request.IsCompleted;
        preset.IsOverdue = request.IsOverdue;
        preset.Priority = request.Priority;
        preset.CategoryIds = request.CategoryIds != null ? JsonSerializer.Serialize(request.CategoryIds) : null;
        preset.TagIds = request.TagIds != null ? JsonSerializer.Serialize(request.TagIds) : null;
        preset.DueDateFrom = request.DueDateFrom;
        preset.DueDateTo = request.DueDateTo;
        preset.CreatedAtFrom = request.CreatedAtFrom;
        preset.CreatedAtTo = request.CreatedAtTo;
        preset.SortBy = request.SortBy;
        preset.SortOrder = request.SortOrder;
        preset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(preset);
    }

    public async Task<bool> DeleteFilterPresetAsync(int presetId, int userId)
    {
        var preset = await _context.FilterPresets
            .FirstOrDefaultAsync(p => p.Id == presetId && p.UserId == userId);

        if (preset == null)
            return false;

        _context.FilterPresets.Remove(preset);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SearchFilterRequest> GetSearchFilterRequestFromPresetAsync(int presetId, int userId)
    {
        var preset = await _context.FilterPresets
            .FirstOrDefaultAsync(p => p.Id == presetId && p.UserId == userId);

        if (preset == null)
            throw new KeyNotFoundException("Filter preset not found");

        return new SearchFilterRequest
        {
            SearchQuery = preset.SearchQuery,
            IsCompleted = preset.IsCompleted,
            IsOverdue = preset.IsOverdue,
            Priority = preset.Priority,
            CategoryIds = preset.CategoryIds != null ? JsonSerializer.Deserialize<List<int>>(preset.CategoryIds) : null,
            TagIds = preset.TagIds != null ? JsonSerializer.Deserialize<List<int>>(preset.TagIds) : null,
            DueDateFrom = preset.DueDateFrom,
            DueDateTo = preset.DueDateTo,
            CreatedAtFrom = preset.CreatedAtFrom,
            CreatedAtTo = preset.CreatedAtTo,
            SortBy = preset.SortBy,
            SortOrder = preset.SortOrder
        };
    }

    private FilterPresetDto MapToDto(FilterPreset preset)
    {
        return new FilterPresetDto
        {
            Id = preset.Id,
            Name = preset.Name,
            SearchQuery = preset.SearchQuery,
            IsCompleted = preset.IsCompleted,
            IsOverdue = preset.IsOverdue,
            Priority = preset.Priority,
            CategoryIds = preset.CategoryIds != null ? JsonSerializer.Deserialize<List<int>>(preset.CategoryIds) : null,
            TagIds = preset.TagIds != null ? JsonSerializer.Deserialize<List<int>>(preset.TagIds) : null,
            DueDateFrom = preset.DueDateFrom,
            DueDateTo = preset.DueDateTo,
            CreatedAtFrom = preset.CreatedAtFrom,
            CreatedAtTo = preset.CreatedAtTo,
            SortBy = preset.SortBy,
            SortOrder = preset.SortOrder,
            CreatedAt = preset.CreatedAt,
            UpdatedAt = preset.UpdatedAt
        };
    }
}

