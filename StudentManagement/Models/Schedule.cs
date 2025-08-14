using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    [Table("Schedules")]
    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        public int TeacherCourseId { get; set; }

        [Required]
        public int DayOfWeek { get; set; } // 2-8

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(20)]
        public string? RoomNumber { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        // Navigation properties
        [ForeignKey("TeacherCourseId")]
        public virtual TeacherCourse TeacherCourse { get; set; } = null!;
    }
}
