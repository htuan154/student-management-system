using StudentManagementSystem.Dtos.Enrollment;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<EnrollmentDto?> GetByIdAsync(int enrollmentId);
        Task<IEnumerable<EnrollmentDto>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<EnrollmentDto>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<EnrollmentDto>> SearchAsync(string searchTerm);
        Task<(IEnumerable<EnrollmentDto> Enrollments, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<EnrollmentDto>> GetUnscoredAsync();
        Task<bool> CreateAsync(EnrollmentCreateDto dto);
        Task<bool> UpdateAsync(EnrollmentUpdateDto dto);
        Task<bool> DeleteAsync(int enrollmentId);
    }
}
