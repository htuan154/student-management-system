using StudentManagementSystem.DTOs.Semester;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface ISemesterService
    {
        Task<IEnumerable<SemesterDto>> GetAllAsync();
        Task<IEnumerable<SemesterDto>> GetActiveSemstersAsync();
        Task<SemesterDto?> GetByIdAsync(int id);
        Task<SemesterDto?> GetSemesterWithClassesAsync(int semesterId);
        Task<SemesterDto?> GetSemesterWithTeacherCoursesAsync(int semesterId);
        Task<SemesterDto?> GetSemesterWithEnrollmentsAsync(int semesterId);
        Task<SemesterDto> CreateAsync(CreateSemesterDto createDto);
        Task<SemesterDto?> UpdateAsync(int id, UpdateSemesterDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<SemesterDto>> GetSemestersByAcademicYearAsync(string academicYear);
        Task<(IEnumerable<SemesterDto> Semesters, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<bool> IsSemesterNameExistsAsync(string semesterName, string academicYear, int? excludeSemesterId = null);
    }
}
