using Microsoft.EntityFrameworkCore;
using REA.Models.Entities;

namespace REA.Models.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<AcademicRecord> AcademicRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(s => s.Email).IsUnique();
                entity.HasIndex(s => new { s.Grade, s.Section });
            });

            // Teacher configuration
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasIndex(t => t.Email).IsUnique();
            });

            // AcademicRecord configuration - ACTUALIZADO
            modelBuilder.Entity<AcademicRecord>(entity =>
            {
                // CAMBIO: Usar Period en lugar de Term
                entity.HasIndex(ar => new { ar.StudentId, ar.Subject, ar.Period, ar.SchoolYear });
                entity.HasIndex(ar => new { ar.TeacherId, ar.Period, ar.SchoolYear });
        
                // CORRECCIÓN: Nueva forma de configurar check constraint
                entity.ToTable(t => t.HasCheckConstraint("CK_AcademicRecords_Period", "\"Period\" BETWEEN 1 AND 4"));
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => new { p.StudentId, p.Status });
                entity.HasIndex(p => p.DueDate);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}