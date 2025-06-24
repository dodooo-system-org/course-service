using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using course_service.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace course_service.Data.Entities;

[Index(nameof(Order), IsUnique = true)]
public class ModuleEntity : BaseEntity
{
    [Key]
    [Column("module_id", TypeName = "char(36)")]
    public Guid ModuleId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("module_name", TypeName = "varchar(100)")]
    public string ModuleName { get; set; } = string.Empty;

    [Required]
    [Column("module_description", TypeName = "text")]
    public string ModuleDescription { get; set; } = string.Empty;

    [Required]
    [Column("order", TypeName = "decimal(10, 5)")]
    public decimal Order { get; set; } = 0;

    [Column("course_id", TypeName = "char(36)")]
    public Guid? CourseId { get; set; }
    [ForeignKey("CourseId")]
    public CourseEntity? Course { get; set; }
}
