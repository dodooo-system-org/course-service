using System;
using System.ComponentModel.DataAnnotations;

namespace course_service.Modules.Modules.DTOs;

public class CreateModuleDto
{
    [Required]
    [MaxLength(100, ErrorMessage = "Module name cannot be longer than 100 characters.")]
    [MinLength(3, ErrorMessage = "Module name must be at least 3 characters long.")]
    public required string ModuleName { get; set; }
    [Required]
    [MinLength(10, ErrorMessage = "Module description must be at least 10 characters long.")]
    [MaxLength(1000, ErrorMessage = "Module description cannot be longer than 1000 characters.")]
    public required string ModuleDescription { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Order must be between 0 and 100.")]
    public required decimal Order { get; set; } = 0;
    [Required]
    public required Guid CourseId { get; set; }
}
