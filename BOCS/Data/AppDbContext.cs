using BOCS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BOCS.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<CourseEnrollment> Enrollments { get; set; } = default!;
        public DbSet<CourseLesson> Lessons { get; set; } = default!;
        public DbSet<CourseSubject> Subjects { get; set; } = default!; // ✅ নতুন
        public DbSet<Users> Users { get; set; } = default!; // ✅ নতুন
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Course =====
            modelBuilder.Entity<Course>(b =>
            {
                b.ToTable("Courses");
                b.HasKey(c => c.Id);

                b.Property(c => c.Title).HasMaxLength(120).IsRequired();
                b.Property(c => c.ThumbnailUrl).HasMaxLength(512);
                b.Property(c => c.DurationDays).HasDefaultValue(30);
                b.Property(c => c.PriceBdt).HasDefaultValue(5000);

                // Course -> Enrollments (1-to-many)
                b.HasMany(c => c.Enrollments)
                 .WithOne(e => e.Course)
                 .HasForeignKey(e => e.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Course -> Subjects (1-to-many) ✅ নতুন
                b.HasMany(c => c.Subjects)
                 .WithOne(s => s.Course)
                 .HasForeignKey(s => s.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Course -> Lessons (direct, optional – যদি Subject না থাকে)
                b.HasMany(c => c.Lessons)
                 .WithOne(l => l.Course)
                 .HasForeignKey(l => l.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(c => c.Title);
            });

            // ===== CourseEnrollment =====
            modelBuilder.Entity<CourseEnrollment>(b =>
            {
                b.ToTable("CourseEnrollments");
                b.HasKey(e => e.Id);

                b.Property(e => e.StudentId).HasMaxLength(450).IsRequired();

                b.Property(e => e.AccessType).HasConversion<int>().IsRequired();
                b.Property(e => e.PaymentMethod).HasConversion<int>().IsRequired();

                b.Property(e => e.TransactionId).HasMaxLength(64);
                b.Property(e => e.SenderNumber).HasMaxLength(20);
                b.Property(e => e.MobileNumber).HasMaxLength(20);

                b.Property(e => e.PriceAtEnrollment).HasDefaultValue(0);
                b.Property(e => e.CreatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");

                b.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique(false);
                b.HasIndex(e => new { e.CourseId, e.IsApproved, e.IsArchived });
            });

            // ===== CourseSubject =====
            modelBuilder.Entity<CourseSubject>(b =>
            {
                b.ToTable("CourseSubjects");
                b.HasKey(s => s.Id);

                b.Property(s => s.Title).HasMaxLength(150).IsRequired();
                b.Property(s => s.SortOrder).HasDefaultValue(0);

                b.HasMany(s => s.Lessons)
                 .WithOne(l => l.Subject)
                 .HasForeignKey(l => l.SubjectId)
                 // ❌ আগে ছিল OnDelete(DeleteBehavior.SetNull)
                 // ✅ পরিবর্তন করো
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(s => new { s.CourseId, s.SortOrder });
            });

            // ===== CourseLesson =====
            modelBuilder.Entity<CourseLesson>(b =>
            {
                b.ToTable("CourseLessons");
                b.HasKey(l => l.Id);

                b.Property(l => l.Title).HasMaxLength(200).IsRequired();
                b.Property(l => l.YoutubeId).HasMaxLength(20).IsRequired();
                b.Property(l => l.YoutubeUrlRaw).HasMaxLength(512);

                b.Property(l => l.SortOrder).HasDefaultValue(0);
                b.Property(l => l.IsPublished).HasDefaultValue(true);
                b.Property(l => l.CreatedAtUtc)
                 .HasDefaultValueSql("GETUTCDATE()");

                // ✅ Subject handled above
                // Direct relation with Course also exists (optional)
                b.HasIndex(l => new { l.CourseId, l.SortOrder });
            });
        }
    }
}