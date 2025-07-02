using StudentManagementSystem.Dtos.TeacherCourse;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface ITeacherCourseService
    {
        Task<TeacherCourseDto?> GetByIdAsync(int teacherCourseId);
        Task<IEnumerable<TeacherCourseDto>> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<TeacherCourseDto>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<TeacherCourseDto>> SearchAsync(string searchTerm);
        Task<(IEnumerable<TeacherCourseDto> TeacherCourses, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> CreateAsync(TeacherCourseCreateDto dto);
        Task<bool> UpdateAsync(TeacherCourseUpdateDto dto);
        Task<bool> DeleteAsync(int teacherCourseId);
    }
}