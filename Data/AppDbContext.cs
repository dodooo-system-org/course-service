using course_service.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace course_service.Data
{
    public class AppDbContext : DbContext
    {

        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<CourseEntity> Courses { get; set; }
        public DbSet<ModuleEntity> Modules { get; set; }
        public DbSet<LessonEntity> Lessons { get; set; }
        public DbSet<LessonPartEntity> LessonParts { get; set; }
        public DbSet<EnrollmentEntity> Enrollments { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // On delete set null for ModuleEntity's CourseId
            modelBuilder.Entity<ModuleEntity>()
                .HasOne(m => m.Course)
                .WithMany()
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CourseEntity>()
                .HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}