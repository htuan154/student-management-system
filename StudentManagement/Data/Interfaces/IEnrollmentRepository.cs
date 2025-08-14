using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(string courseId);
        Task<IEnumerable<Enrollment>> GetByTeacherCourseIdAsync(int teacherCourseId);
        Task<IEnumerable<Enrollment>> GetBySemesterIdAsync(int semesterId);
        Task<IEnumerable<Enrollment>> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<Enrollment>> GetByStatusAsync(string status);
        Task<IEnumerable<Enrollment>> SearchAsync(string searchTerm);
        Task<(IEnumerable<Enrollment> Enrollments, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Enrollment>> GetUnscoredAsync();
        Task<IEnumerable<Enrollment>> GetStudentEnrollmentsWithScoresAsync(string studentId);
        
        Task<IEnumerable<Enrollment>> GetUnscoredEnrollmentsByTeacherCourseAsync(int teacherCourseId);
        Task<IEnumerable<Enrollment>> GetUnscoredEnrollmentsForClassAsync(string courseId, int teacherCourseId);
        
        Task<bool> IsStudentEnrolledAsync(string studentId, string courseId, int semesterId);
        Task<bool> IsStudentEnrolledInCourseAsync(string studentId, string courseId, int semesterId);
        Task<bool> CanDeleteEnrollmentAsync(int enrollmentId);
        
        // ✅ THÊM - Statistics methods
        Task<int> GetEnrollmentCountByCourseAsync(string courseId, int semesterId);
        Task<int> GetEnrollmentCountByTeacherCourseAsync(int teacherCourseId);
        Task<int> GetActiveEnrollmentCountBySemesterAsync(int semesterId);
        
        // ✅ THÊM - Utility methods
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAndSemesterAsync(string studentId, int semesterId);
    }
}
