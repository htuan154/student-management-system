using StudentManagementSystem.DTOs.TeacherCourse;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface ITeacherCourseService
    {
        Task<TeacherCourseDto?> GetByIdAsync(int teacherCourseId);
        Task<IEnumerable<TeacherCourseDto>> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<TeacherCourseDto>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<TeacherCourseDto>> GetBySemesterIdAsync(int semesterId);
        Task<bool> CreateAsync(TeacherCourseCreateDto dto);
        Task<bool> UpdateAsync(TeacherCourseUpdateDto dto);
        Task<bool> DeleteAsync(int teacherCourseId);
        Task<IEnumerable<TeacherCourseDto>> SearchAsync(string searchTerm);
        Task<(IEnumerable<TeacherCourseDto> TeacherCourses, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> IsTeacherCourseExistsAsync(string teacherId, string courseId, int semesterId, int? excludeId = null);
        Task<int> GetTeacherWorkloadAsync(string teacherId, int semesterId);
    }
}
