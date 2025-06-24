using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using course_service.Shared.Entities;

namespace course_service.Data.Entities;

public enum CategoryStatus
{
    Active,
    Suspending
}

public class CategoryEntity : BaseEntity
{
    [Key]
    [Column("category_id", TypeName = "char(36)")]
    public Guid CategoryId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("category_name", TypeName = "varchar(100)")]
    public string CategoryName { get; set; } = string.Empty;

    [Required]
    [Column("category_description", TypeName = "text")]
    public string CategoryDescription { get; set; } = string.Empty;

    [Required]
    [Column("category_image_url", TypeName = "varchar(255)")]
    public string? CategoryImageUrl { get; set; } = null;

    [Column("status", TypeName = "varchar(20)")]
    public CategoryStatus Status { get; set; } = CategoryStatus.Active;
}
