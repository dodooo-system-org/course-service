using System;

namespace course_service.Modules.Category.DTOs;

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryDescription { get; set; } = string.Empty;
    public string CategoryImageUrl { get; set; } = string.Empty;
}
