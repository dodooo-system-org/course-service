using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using course_service.Shared.Entities;

namespace course_service.Data.Entities;

public class LessonPartEntity : BaseEntity
{
    [Key]
    [Column("lesson_part_id", TypeName = "char(36)")]
    public Guid LessonPartId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("lesson_part_name", TypeName = "varchar(100)")]
    public string LessonPartName { get; set; } = string.Empty;

    [Required]
    [Column("lesson_part_content", TypeName = "text")]
    /// <summary>
    /// Markdown content for the lesson part.
    /// </summary>
    public string LessonPartContent { get; set; } = string.Empty;

    [Column("order", TypeName = "decimal(10, 5)")]
    public decimal Order { get; set; } = 0;

    [Column("lesson_id", TypeName = "char(36)")]
    public Guid? LessonId { get; set; } = null;
    [ForeignKey("LessonId")]
    public LessonEntity? Lesson { get; set; } = null;
}
