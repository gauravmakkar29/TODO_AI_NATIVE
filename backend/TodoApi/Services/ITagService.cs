using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagByIdAsync(int tagId);
    Task<TagDto> CreateTagAsync(CreateTagRequest request);
    Task<TagDto?> UpdateTagAsync(int tagId, UpdateTagRequest request);
    Task<bool> DeleteTagAsync(int tagId);
}

