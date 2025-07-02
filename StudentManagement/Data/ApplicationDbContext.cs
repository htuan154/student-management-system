using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;
namespace StudentManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Role> Roles { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<TeacherCourse> TeacherCourses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Score> Scores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraints
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.Email)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure check constraints for User table
            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_User_OneRoleType",
                    "([StudentId] IS NOT NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NOT NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NOT NULL)"
                ));

            // Configure unique constraints for TeacherCourses
            modelBuilder.Entity<TeacherCourse>()
                .HasIndex(tc => new { tc.TeacherId, tc.CourseId, tc.Semester, tc.Year })
                .IsUnique();

            // Configure unique constraints for Enrollments
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId, e.Semester, e.Year })
                .IsUnique();

            // Configure check constraint for Enrollment Status
            modelBuilder.Entity<Enrollment>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Enrollment_Status",
                    "[Status] IN ('Enrolled', 'Dropped', 'Completed')"
                ));

            // Configure unique constraint for Scores (one score per enrollment)
            modelBuilder.Entity<Score>()
                .HasIndex(s => s.EnrollmentId)
                .IsUnique();

            // Configure computed columns for Score
            modelBuilder.Entity<Score>()
                .Property(s => s.TotalScore)
                .HasComputedColumnSql(
                    "CASE " +
                    "WHEN [ProcessScore] IS NOT NULL AND [MidtermScore] IS NOT NULL AND [FinalScore] IS NOT NULL " +
                    "THEN ROUND(([ProcessScore] * 0.2) + ([MidtermScore] * 0.3) + ([FinalScore] * 0.5), 2) " +
                    "ELSE NULL " +
                    "END"
                );

            modelBuilder.Entity<Score>()
                .Property(s => s.IsPassed)
                .HasComputedColumnSql(
                    "CASE " +
                    "WHEN ([ProcessScore] * 0.2) + ([MidtermScore] * 0.3) + ([FinalScore] * 0.5) >= 4.0 THEN CAST(1 AS BIT) " +
                    "WHEN [ProcessScore] IS NOT NULL AND [MidtermScore] IS NOT NULL AND [FinalScore] IS NOT NULL THEN CAST(0 AS BIT) " +
                    "ELSE NULL " +
                    "END"
                );

            // Configure cascade delete relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithMany(e => e.Users)
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Teacher)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tc => tc.Teacher)
                .WithMany(t => t.TeacherCourses)
                .HasForeignKey(tc => tc.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tc => tc.Course)
                .WithMany(c => c.TeacherCourses)
                .HasForeignKey(tc => tc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Teacher)
                .WithMany(t => t.Enrollments)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid multiple cascade paths

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Enrollment)
                .WithOne(e => e.Score)
                .HasForeignKey<Score>(s => s.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
