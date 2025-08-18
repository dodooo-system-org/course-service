using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using course_service.Data.Entities;
using course_service.Shared.DTOs;

namespace course_service.Modules.Course.DTOs;

public class GetDeletedCourseDto : PaginationDto
{
    [JsonPropertyName("categoryId")]
    public Guid? CategoryId { get; set; }

    [JsonPropertyName("courseLevel")]
    [EnumDataType(typeof(CourseLevel), ErrorMessage = "Invalid course level")]
    public CourseLevel? CourseLevel { get; set; }

    // [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    // [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
}
