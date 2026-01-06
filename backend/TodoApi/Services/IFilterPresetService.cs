using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface IFilterPresetService
{
    Task<IEnumerable<FilterPresetDto>> GetFilterPresetsByUserIdAsync(int userId);
    Task<FilterPresetDto?> GetFilterPresetByIdAsync(int presetId, int userId);
    Task<FilterPresetDto> CreateFilterPresetAsync(CreateFilterPresetRequest request, int userId);
    Task<FilterPresetDto?> UpdateFilterPresetAsync(int presetId, UpdateFilterPresetRequest request, int userId);
    Task<bool> DeleteFilterPresetAsync(int presetId, int userId);
    Task<SearchFilterRequest> GetSearchFilterRequestFromPresetAsync(int presetId, int userId);
}

