using System;
using System.ComponentModel.DataAnnotations;

namespace course_service.Modules.Category.DTOs;

public class CreateCategoryDto
{
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    [Required(ErrorMessage = "Category name is required.")]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Category description cannot exceed 500 characters.")]
    [MinLength(10, ErrorMessage = "Category description must be at least 10 characters long.")]
    [Required(ErrorMessage = "Category description is required.")]
    public string CategoryDescription { get; set; } = string.Empty;

    [Url(ErrorMessage = "Category image URL must be a valid URL.")]
    [Required(ErrorMessage = "Category image URL is required.")]
    public string CategoryImageUrl { get; set; } = string.Empty;
}
