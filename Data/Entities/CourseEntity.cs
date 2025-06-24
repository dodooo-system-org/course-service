using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using course_service.Shared.Entities;

namespace course_service.Data.Entities;

public enum CourseLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Master
}

public class CourseEntity : BaseEntity
{
    [Key]
    [Column("course_id", TypeName = "char(36)")]
    public Guid CourseId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("course_name", TypeName = "varchar(100)")]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    [Column("course_description", TypeName = "text")]
    public string CourseDescription { get; set; } = string.Empty;

    [Required]
    [Column("level", TypeName = "int")]
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [Column("course_image_url", TypeName = "varchar(255)")]
    public string? CourseImageUrl { get; set; }

    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public required CategoryEntity Category { get; set; }
}
