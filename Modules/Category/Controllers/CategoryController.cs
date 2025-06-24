using course_service.Modules.Category.DTOs;
using course_service.Modules.Category.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace course_service.Modules.Category.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOneAsync([FromBody] CreateCategoryDto category)
        {
            var newCategory = await _categoryService.CreateOneAsync(category);
            return Ok(newCategory);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOneAsync([FromRoute] Guid id)
        {
            var category = await _categoryService.GetOneAsync(id);
            return Ok(category);
        }

        [HttpGet]
        public async Task<IActionResult> GetListAsync()
        {
            var categories = await _categoryService.GetListAsync();
            return Ok(categories);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOneAsync([FromRoute] Guid id, [FromBody] UpdateCategoryDto category)
        {
            var updatedCategory = await _categoryService.UpdateOneAsync(id, category);
            return Ok(updatedCategory);
        }
    }
}
