using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using course_service.Data.Entities;

namespace course_service.Modules.Course.DTOs;

public class CreateCourseDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Course name must be at least 3 characters long.")]
    [MaxLength(100, ErrorMessage = "Course name cannot exceed 100 characters.")]
    public required string CourseName { get; set; }

    [Required]
    [MinLength(10, ErrorMessage = "Course description must be at least 10 characters long.")]
    [MaxLength(1000, ErrorMessage = "Course description cannot exceed 1000 characters.")]
    public required string CourseDescription { get; set; }

    [Required]
    [EnumDataType(typeof(CourseLevel), ErrorMessage = "Invalid course level.")]
    public required CourseLevel CourseLevel { get; set; }

    [Url(ErrorMessage = "Invalid URL format.")]
    public string? CourseImageUrl { get; set; }

    [Required(ErrorMessage = "Category ID is required.")]
    public Guid CategoryId { get; set; }

}
