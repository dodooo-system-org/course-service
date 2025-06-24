using System.ComponentModel.DataAnnotations.Schema;

namespace course_service.Shared.Entities
{
    public abstract class BaseEntity
    {
        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; } = null;

        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; } = null;
    }
}