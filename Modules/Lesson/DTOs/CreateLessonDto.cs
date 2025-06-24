using System;
using System.ComponentModel.DataAnnotations;

namespace course_service.Modules.Lesson.DTOs;

public class CreateLessonDto
{
    [Required(ErrorMessage = "Lesson Id is required.")]
    [StringLength(100, ErrorMessage = "Lesson name cannot exceed 100 characters.")]
    public required string LessonName { get; set; }

    [Required(ErrorMessage = "Lesson description is required.")]
    [MaxLength(1000, ErrorMessage = "Lesson description cannot exceed 500 characters.")]
    [MinLength(10, ErrorMessage = "Lesson description must be at least 10")]
    public required string LessonDescription { get; set; }

    [Required(ErrorMessage = "Order is required.")]
    [Range(0, 10000, ErrorMessage = "Order must be between 0 and 100000.")]
    public decimal Order { get; set; }

    [Required(ErrorMessage = "Duration is required.")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes (24 hours).")]
    public int Duration { get; set; }

    [Required(ErrorMessage = "ModuleId is required.")]
    public Guid ModuleId { get; set; }
}
