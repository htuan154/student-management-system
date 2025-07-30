using StudentManagementSystem.DTOs.Teacher;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync();
        Task<TeacherResponseDto?> GetTeacherByIdAsync(string teacherId);
        Task<TeacherDetailResponseDto?> GetTeacherDetailByIdAsync(string teacherId);
        Task<TeacherResponseDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto);
        Task<TeacherResponseDto?> UpdateTeacherAsync(string teacherId, UpdateTeacherDto updateTeacherDto);
        Task<bool> DeleteTeacherAsync(string teacherId);
        Task<IEnumerable<TeacherResponseDto>> GetTeachersByDepartmentAsync(string department);
        Task<IEnumerable<TeacherResponseDto>> GetTeachersByDegreeAsync(string degree);
        Task<IEnumerable<TeacherResponseDto>> SearchTeachersAsync(string searchTerm);
        Task<(IEnumerable<TeacherResponseDto> Teachers, int TotalCount)> GetPagedTeachersAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<TeacherResponseDto>> GetTeachersBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary);
        Task<IEnumerable<TeacherResponseDto>> GetTeachersWithActiveCoursesAsync();
        Task<bool> IsEmailExistsAsync(string email, string? excludeTeacherId = null);
        Task<bool> IsTeacherIdExistsAsync(string teacherId);
        Task<IEnumerable<string>> GetDistinctDepartmentsAsync();
        Task<IEnumerable<string>> GetDistinctDegreesAsync();
        Task<IEnumerable<TeacherResponseDto>> GetTeachersByCourseIdAsync(string courseId);
    }
}
