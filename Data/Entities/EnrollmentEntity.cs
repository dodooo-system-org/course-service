using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using course_service.Shared.Entities;

namespace course_service.Data.Entities;

public class EnrollmentEntity : BaseEntity
{
    [Key]
    [Column("enrollment_id", TypeName = "char(36)")]
    public Guid EnrollmentId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id", TypeName = "char(36)")]
    public Guid UserId { get; set; }

    [Required]
    [Column("course_id", TypeName = "char(36)")]
    public Guid CourseId { get; set; }
    [ForeignKey("CourseId")]
    public CourseEntity Course { get; set; } = null!;


    [Required]
    [Column("progress", TypeName = "decimal(5, 2)")]
    public decimal Progress { get; set; } = 0.0m; // Progress in percentage (0.00 to 100.00)

    [Column("completion_date", TypeName = "datetime")]
    public DateTime? CompletionDate { get; set; } = null;
}
