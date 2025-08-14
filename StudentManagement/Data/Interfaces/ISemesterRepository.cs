using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface ISemesterRepository : IRepository<Semester>
    {
        Task<IEnumerable<Semester>> GetActiveSemstersAsync();
        Task<Semester?> GetSemesterWithClassesAsync(int semesterId);
        Task<Semester?> GetSemesterWithTeacherCoursesAsync(int semesterId);
        Task<Semester?> GetSemesterWithEnrollmentsAsync(int semesterId);
        Task<bool> IsSemesterNameExistsAsync(string semesterName, string academicYear, int? excludeSemesterId = null);
        Task<IEnumerable<Semester>> GetSemestersByAcademicYearAsync(string academicYear);
        Task<(IEnumerable<Semester> Semesters, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<bool> CanDeleteSemesterAsync(int semesterId);
    }
}
