using StudentManagementSystem.DTOs.Class;

namespace StudentManagementSystem.Services.Interfaces
{
    public interface IClassService
    {
        Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync();
        Task<ClassResponseDto?> GetClassByIdAsync(string classId);
        Task<ClassWithStudentsDto?> GetClassWithStudentsAsync(string classId);
        Task<ClassResponseDto> CreateClassAsync(CreateClassDto createClassDto);
        Task<ClassResponseDto?> UpdateClassAsync(string classId, UpdateClassDto updateClassDto);
        Task<bool> DeleteClassAsync(string classId);
        Task<IEnumerable<ClassResponseDto>> GetActiveClassesAsync();
        Task<IEnumerable<ClassResponseDto>> SearchClassesAsync(string searchTerm);
        Task<(IEnumerable<ClassResponseDto> Classes, int TotalCount)> GetPagedClassesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<IEnumerable<ClassResponseDto>> GetClassesByMajorAsync(string major);
        Task<IEnumerable<ClassResponseDto>> GetClassesByAcademicYearAsync(string academicYear);
        Task<bool> IsClassIdExistsAsync(string classId);
        Task<bool> IsClassNameExistsAsync(string className, string? excludeClassId = null);
        Task<bool> CanDeleteClassAsync(string classId);
    }
}
