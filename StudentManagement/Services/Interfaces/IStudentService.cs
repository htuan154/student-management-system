using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync();
        Task<StudentResponseDto?> GetStudentByIdAsync(string studentId);
        Task<StudentResponseDto> CreateStudentAsync(CreateStudentDto createStudentDto);
        Task<StudentResponseDto?> UpdateStudentAsync(string studentId, UpdateStudentDto updateStudentDto);
        Task<bool> DeleteStudentAsync(string studentId);
        Task<IEnumerable<StudentResponseDto>> GetStudentsByClassIdAsync(string classId);
        Task<IEnumerable<StudentResponseDto>> SearchStudentsAsync(string searchTerm);
        Task<(IEnumerable<StudentResponseDto> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageNumber, int pageSize, string? searchTerm = null);
        Task<bool> IsEmailExistsAsync(string email, string? excludeStudentId = null);
        Task<bool> IsStudentIdExistsAsync(string studentId);

    }
}
