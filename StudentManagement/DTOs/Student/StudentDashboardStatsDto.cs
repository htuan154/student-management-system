using System.Collections.Generic;

namespace StudentManagementSystem.DTOs.Student
{
    // DTO này chứa các thông tin thống kê cho trang Dashboard của sinh viên
    public class StudentDashboardStatsDto
    {
        public int TotalRegisteredCourses { get; set; }
        public int CompletedCourses { get; set; }
        public double? Gpa { get; set; } // Điểm trung bình, có thể là null
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
    }

    // DTO phụ để chứa thông tin về một hoạt động gần đây
    public class RecentActivityDto
    {
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string? Semester { get; set; }
        public int? Year { get; set; }
        public string? Status { get; set; }
    }
}
