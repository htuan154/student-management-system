using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Student?> GetStudentWithClassAsync(string studentId)
        {
            return await _dbSet
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<IEnumerable<Student>> GetStudentsByClassIdAsync(string classId)
        {
            return await _dbSet
                .Include(s => s.Class)
                .Where(s => s.ClassId == classId)
                .ToListAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeStudentId = null)
        {
            var query = _dbSet.Where(s => s.Email == email);

            if (!string.IsNullOrEmpty(excludeStudentId))
            {
                query = query.Where(s => s.StudentId != excludeStudentId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsStudentIdExistsAsync(string studentId)
        {
            return await _dbSet.AnyAsync(s => s.StudentId == studentId);
        }

        public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
        {
            return await _dbSet
                .Include(s => s.Class)
                .Where(s => s.FullName.Contains(searchTerm) ||
                           s.StudentId.Contains(searchTerm) ||
                           s.Email.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet.Include(s => s.Class).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.FullName.Contains(searchTerm) ||
                                        s.StudentId.Contains(searchTerm) ||
                                        s.Email.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var students = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (students, totalCount);
        }
    }
}
