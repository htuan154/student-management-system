namespace StudentManagementSystem.DTOs.Semester
{
    public class SemesterDto
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
