using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using course_service.Shared.Entities;

namespace course_service.Data.Entities;

public class LessonEntity : BaseEntity
{
    [Key]
    [Column("lesson_id", TypeName = "char(36)")]
    public Guid LessonId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("lesson_name", TypeName = "varchar(100)")]
    public string LessonName { get; set; } = string.Empty;

    [Required]
    [Column("lesson_description", TypeName = "text")]
    public string LessonDescription { get; set; } = string.Empty;

    [Required]
    [Column("order", TypeName = "decimal(10, 5)")]
    public decimal Order { get; set; } = 0;

    [Required]
    [Column("duration", TypeName = "int")]
    public int Duration { get; set; } = 0; // Duration in minutes

    [Column("module_id", TypeName = "char(36)")]
    public Guid? ModuleId { get; set; } = null;
    [ForeignKey("ModuleId")]
    public ModuleEntity? Module { get; set; } = null;
}
