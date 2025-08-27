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
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementDetail> AnnouncementDetails { get; set; }

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

            // Configure check constraints
            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_User_OneRoleType",
                    "([StudentId] IS NOT NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NOT NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NOT NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NULL)" // Admin users
                ));

            // Configure unique constraints for TeacherCourses
            modelBuilder.Entity<TeacherCourse>()
                .HasIndex(tc => new { tc.TeacherId, tc.CourseId, tc.SemesterId })
                .IsUnique();

            // Configure unique constraints for Enrollments
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId, e.SemesterId })
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

            // BỔ SUNG CẤU HÌNH CHO TRIGGER
            // Thông báo cho EF Core biết bảng "Scores" có trigger, để nó sử dụng
            // một phương pháp lưu dữ liệu khác tương thích hơn.
            modelBuilder.Entity<Score>()
                .ToTable(tb => tb.HasTrigger("trg_Score_ComputeColumns")); // Bạn có thể đặt tên trigger bất kỳ

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

            // SỬA computed column IsPassed - thêm explicit CAST
            modelBuilder.Entity<Score>()
                .Property(s => s.IsPassed)
                .HasComputedColumnSql(
                    "CASE " +
                    "WHEN ([ProcessScore] * 0.2) + ([MidtermScore] * 0.3) + ([FinalScore] * 0.5) >= 4.0 " +
                    "THEN CONVERT(BIT, 1) " + // ⬅ THAY ĐỔI: explicit CONVERT
                    "WHEN [ProcessScore] IS NOT NULL AND [MidtermScore] IS NOT NULL AND [FinalScore] IS NOT NULL " +
                    "THEN CONVERT(BIT, 0) " + // ⬅ THAY ĐỔI: explicit CONVERT
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
                .HasOne(e => e.TeacherCourse)
                .WithMany(tc => tc.Enrollments)
                .HasForeignKey(e => e.TeacherCourseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Enrollment)
                .WithOne(e => e.Score)
                .HasForeignKey<Score>(s => s.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Semester>()
                .HasIndex(s => new { s.SemesterName, s.AcademicYear })
                .IsUnique();


            modelBuilder.Entity<Schedule>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Schedule_DayOfWeek",
                    "[DayOfWeek] BETWEEN 2 AND 8"
                ));

            modelBuilder.Entity<Schedule>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Schedule_TimeRange",
                    "[StartTime] < [EndTime]"
                ));

            // THÊM CẤU HÌNH CHO ANNOUNCEMENT DETAIL
            modelBuilder.Entity<AnnouncementDetail>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_AnnouncementDetail_AtLeastOneTarget",
                    "[RoleId] IS NOT NULL OR [ClassId] IS NOT NULL OR [CourseId] IS NOT NULL OR [UserId] IS NOT NULL"
                ));

            // THÊM RELATIONSHIPS CHO SEMESTER
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Semester)
                .WithMany(s => s.Classes)
                .HasForeignKey(c => c.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tc => tc.Semester)
                .WithMany(s => s.TeacherCourses)
                .HasForeignKey(tc => tc.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Semester)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // SỬA - TeacherCourse relationship trong Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.TeacherCourse)
                .WithMany(tc => tc.Enrollments)
                .HasForeignKey(e => e.TeacherCourseId)
                .OnDelete(DeleteBehavior.SetNull);

            // THÊM RELATIONSHIPS CHO SCHEDULE
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.TeacherCourse)
                .WithMany(tc => tc.Schedules)
                .HasForeignKey(s => s.TeacherCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // THÊM RELATIONSHIPS CHO ANNOUNCEMENT
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.User)
                .WithMany(u => u.Announcements)
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // THÊM RELATIONSHIPS CHO ANNOUNCEMENT DETAIL
            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Announcement)
                .WithMany(a => a.AnnouncementDetails)
                .HasForeignKey(ad => ad.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Role)
                .WithMany(r => r.AnnouncementDetails)
                .HasForeignKey(ad => ad.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Class)
                .WithMany(c => c.AnnouncementDetails)
                .HasForeignKey(ad => ad.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Course)
                .WithMany(c => c.AnnouncementDetails)
                .HasForeignKey(ad => ad.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.User)
                .WithMany(u => u.AnnouncementDetails)
                .HasForeignKey(ad => ad.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // SỬA - User constraint (cho phép Admin users)
            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_User_OneRoleType",
                    "([StudentId] IS NOT NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NOT NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NOT NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NULL)"
                ));

            // Configure unique constraints for TeacherCourses
            modelBuilder.Entity<TeacherCourse>()
                .HasIndex(tc => new { tc.TeacherId, tc.CourseId, tc.SemesterId })
                .IsUnique();

            // Configure unique constraints for Enrollments
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId, e.SemesterId })
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

            // BỔ SUNG CẤU HÌNH CHO TRIGGER
            // Thông báo cho EF Core biết bảng "Scores" có trigger, để nó sử dụng
            // một phương pháp lưu dữ liệu khác tương thích hơn.
            modelBuilder.Entity<Score>()
                .ToTable(tb => tb.HasTrigger("trg_Score_ComputeColumns")); // Bạn có thể đặt tên trigger bất kỳ

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

            // SỬA computed column IsPassed - thêm explicit CAST
            modelBuilder.Entity<Score>()
                .Property(s => s.IsPassed)
                .HasComputedColumnSql(
                    "CASE " +
                    "WHEN ([ProcessScore] * 0.2) + ([MidtermScore] * 0.3) + ([FinalScore] * 0.5) >= 4.0 " +
                    "THEN CONVERT(BIT, 1) " + // ⬅ THAY ĐỔI: explicit CONVERT
                    "WHEN [ProcessScore] IS NOT NULL AND [MidtermScore] IS NOT NULL AND [FinalScore] IS NOT NULL " +
                    "THEN CONVERT(BIT, 0) " + // ⬅ THAY ĐỔI: explicit CONVERT
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
                .HasOne(e => e.TeacherCourse)
                .WithMany(tc => tc.Enrollments)
                .HasForeignKey(e => e.TeacherCourseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Enrollment)
                .WithOne(e => e.Score)
                .HasForeignKey<Score>(s => s.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Semester>()
                .HasIndex(s => new { s.SemesterName, s.AcademicYear })
                .IsUnique();


            modelBuilder.Entity<Schedule>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Schedule_DayOfWeek",
                    "[DayOfWeek] BETWEEN 2 AND 8"
                ));

            modelBuilder.Entity<Schedule>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Schedule_TimeRange",
                    "[StartTime] < [EndTime]"
                ));

            modelBuilder.Entity<AnnouncementDetail>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_AnnouncementDetail_AtLeastOneTarget",
                    "[RoleId] IS NOT NULL OR [ClassId] IS NOT NULL OR [CourseId] IS NOT NULL OR [UserId] IS NOT NULL"
                ));

            // THÊM RELATIONSHIPS CHO SEMESTER
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Semester)
                .WithMany(s => s.Classes)
                .HasForeignKey(c => c.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherCourse>()
                .HasOne(tc => tc.Semester)
                .WithMany(s => s.TeacherCourses)
                .HasForeignKey(tc => tc.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Semester)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // SỬA - TeacherCourse relationship trong Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.TeacherCourse)
                .WithMany(tc => tc.Enrollments)
                .HasForeignKey(e => e.TeacherCourseId)
                .OnDelete(DeleteBehavior.SetNull);

            // THÊM RELATIONSHIPS CHO SCHEDULE
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.TeacherCourse)
                .WithMany(tc => tc.Schedules)
                .HasForeignKey(s => s.TeacherCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // THÊM RELATIONSHIPS CHO ANNOUNCEMENT
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.User)
                .WithMany(u => u.Announcements)
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // THÊM RELATIONSHIPS CHO ANNOUNCEMENT DETAIL
            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Announcement)
                .WithMany(a => a.AnnouncementDetails)
                .HasForeignKey(ad => ad.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Role)
                .WithMany(r => r.AnnouncementDetails)
                .HasForeignKey(ad => ad.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Class)
                .WithMany(c => c.AnnouncementDetails)
                .HasForeignKey(ad => ad.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.Course)
                .WithMany(c => c.AnnouncementDetails)
                .HasForeignKey(ad => ad.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnnouncementDetail>()
                .HasOne(ad => ad.User)
                .WithMany(u => u.AnnouncementDetails)
                .HasForeignKey(ad => ad.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // SỬA - User constraint (cho phép Admin users)
            modelBuilder.Entity<User>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_User_OneRoleType",
                    "([StudentId] IS NOT NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NOT NULL AND [TeacherId] IS NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NOT NULL) OR " +
                    "([StudentId] IS NULL AND [EmployeeId] IS NULL AND [TeacherId] IS NULL)"
                ));

            modelBuilder.Entity<Score>(entity =>
            {
                entity.Ignore(e => e.TotalScore);
                entity.Ignore(e => e.IsPassed);

            });
        }
    }
}
