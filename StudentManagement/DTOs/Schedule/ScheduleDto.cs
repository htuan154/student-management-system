namespace StudentManagementSystem.DTOs.Schedule
{
    public class ScheduleDto
    {
        public int ScheduleId { get; set; }
        public int TeacherCourseId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
        public string? Location { get; set; }


        public string? TeacherName { get; set; }
        public string? CourseName { get; set; }
        public string? CourseId { get; set; }
        public string? SemesterName { get; set; }
        public string? AcademicYear { get; set; }


        public string DayOfWeekName => GetDayOfWeekName(DayOfWeek);
        public string TimeSlot => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

        private static string GetDayOfWeekName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                2 => "Thứ 2",
                3 => "Thứ 3",
                4 => "Thứ 4",
                5 => "Thứ 5",
                6 => "Thứ 6",
                7 => "Thứ 7",
                8 => "Chủ nhật",
                _ => "N/A"
            };
        }
    }
}
