using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs.Schedule
{
    public class UpdateScheduleDto
    {
        public int ScheduleId { get; set; }

        [Required]
        public int TeacherCourseId { get; set; }

        [Required]
        [Range(2, 8, ErrorMessage = "DayOfWeek must be between 2 (Monday) and 8 (Sunday)")]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(20)]
        public string? RoomNumber { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        // Custom validation
        public bool IsValidTimeRange => StartTime < EndTime;
    }
}
