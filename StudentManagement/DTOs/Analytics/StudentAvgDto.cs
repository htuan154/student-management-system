namespace StudentManagementSystem.DTOs.Analytics
{
    public class StudentAvgDto
    {
        public string StudentId { get; set; } = "";
        public string FullName  { get; set; } = "";
        public string ClassName { get; set; } = "";
        public decimal AverageScore { get; set; }
    }
}
