using System;
using System.Collections.Generic;
using course_service.Data.Entities;
using course_service.Modules.Category.DTOs;
using course_service.Shared.DTOs;

namespace course_service.Modules.Category.Interfaces;

public interface ICategoryService
{
    Task<CategoryEntity> CreateOneAsync(CreateCategoryDto category);
    Task<CategoryEntity> GetOneAsync(Guid id);
    Task<MetaPaginationDto<List<CategoryEntity>>> GetListAsync(GetListCategoryDto pagination);
    Task<CategoryEntity> UpdateOneAsync(Guid id, UpdateCategoryDto category);
    Task<IEnumerable<CategoryEntity>> GetAvailableCategoriesAsync();
    Task<MetaPaginationDto<List<CategoryEntity>>> GetDeletedCategoriesAsync(GetDeletedCategoriesDto dto);
}
