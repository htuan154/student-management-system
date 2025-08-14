using StudentManagementSystem.DTOs.Student;

namespace StudentManagementSystem.DTOs.Class
{
    public class ClassWithStudentsDto
    {
        public string ClassId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;

        // ✅ THÊM - Properties còn thiếu
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public string? AcademicYear { get; set; }

        public bool IsActive { get; set; }
        public IEnumerable<StudentResponseDto> Students { get; set; } = new List<StudentResponseDto>();
        public int StudentCount { get; set; }
    }
}
