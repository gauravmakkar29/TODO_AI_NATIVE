using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int categoryId);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryDto?> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int categoryId);
}

