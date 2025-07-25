using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using course_service.Data.Entities;
using course_service.Shared.DTOs;

namespace course_service.Modules.Course.DTOs;

public class AllCourseQueryDto : PaginationDto
{
    [JsonPropertyName("categoryId")]
    public Guid? CategoryId { get; set; }

    [JsonPropertyName("courseLevel")]
    [EnumDataType(typeof(CourseLevel), ErrorMessage = "Invalid course level")]
    public CourseLevel? CourseLevel { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("isDeleted")]
    public bool? IsDeleted { get; set; }
}
