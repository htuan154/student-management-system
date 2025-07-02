using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IClassRepository : IRepository<Class>
    {
        Task<Class?> GetClassWithStudentsAsync(string classId);
        Task<IEnumerable<Class>> GetActiveClassesAsync();
        Task<bool> IsClassIdExistsAsync(string classId);
        Task<bool> IsClassNameExistsAsync(string className, string? excludeClassId = null);
        Task<IEnumerable<Class>> SearchClassesAsync(string searchTerm);
        Task<(IEnumerable<Class> Classes, int TotalCount)> GetPagedClassesAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<IEnumerable<Class>> GetClassesByMajorAsync(string major);
        Task<IEnumerable<Class>> GetClassesByAcademicYearAsync(string academicYear);
        Task<int> GetStudentCountInClassAsync(string classId);
    }
}
