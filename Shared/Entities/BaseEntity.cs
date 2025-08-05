using System.ComponentModel.DataAnnotations.Schema;

namespace course_service.Shared.Entities
{
    public abstract class BaseEntity
    {
        [Column("is_active", TypeName = "tinyint")]
        public bool IsActive { get; set; } = true;

        [Column("is_deleted", TypeName = "tinyint")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; } = null;
    }
}