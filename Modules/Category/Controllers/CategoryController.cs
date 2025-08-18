using course_service.Attributes;
using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Interfaces;
using course_service.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace course_service.Modules.Category.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [RequireAdmin]
        public async Task<IActionResult> CreateOneAsync([FromBody] CreateCategoryDto category)
        {
            var newCategory = await _categoryService.CreateOneAsync(category);
            return Ok(newCategory);
        }

        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableCategoriesAsync()
        {
            var categories = await _categoryService.GetAvailableCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("delete")]
        [RequireAdmin]
        public async Task<IActionResult> GetDeletedCategories([FromQuery] GetDeletedCategoriesDto dto)
        {
            var deletedCategories = await _categoryService.GetDeletedCategoriesAsync(dto);
            return Ok(deletedCategories);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOneAsync([FromRoute] Guid id)
        {
            var category = await _categoryService.GetOneAsync(id);
            return Ok(category);
        }

        [HttpGet]
        [RequireAdmin]
        public async Task<IActionResult> GetListAsync([FromQuery] GetListCategoryDto queries)
        {
            var categories = await _categoryService.GetListAsync(queries);
            return Ok(categories);
        }

        [HttpPut("{id}")]
        [RequireAdmin]
        public async Task<IActionResult> UpdateOneAsync([FromRoute] Guid id, [FromBody] UpdateCategoryDto category)
        {
            var updatedCategory = await _categoryService.UpdateOneAsync(id, category);
            return Ok(updatedCategory);
        }



    }
}
