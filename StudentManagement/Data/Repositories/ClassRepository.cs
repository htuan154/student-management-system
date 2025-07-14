using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data.Repositories
{
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        public ClassRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Class?> GetClassWithStudentsAsync(string classId)
        {
            return await _dbSet
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassId == classId);
        }

        public async Task<IEnumerable<Class>> GetActiveClassesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsClassIdExistsAsync(string classId)
        {
            return await _dbSet.AnyAsync(c => c.ClassId == classId);
        }

        public async Task<bool> IsClassNameExistsAsync(string className, string? excludeClassId = null)
        {
            var query = _dbSet.Where(c => c.ClassName == className);

            if (!string.IsNullOrEmpty(excludeClassId))
            {
                query = query.Where(c => c.ClassId != excludeClassId);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Class>> SearchClassesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => c.ClassName.Contains(searchTerm) ||
                           c.ClassId.Contains(searchTerm) ||
                           c.Major.Contains(searchTerm) ||
                           (c.AcademicYear != null && c.AcademicYear.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Class> Classes, int TotalCount)> GetPagedClassesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.ClassName.Contains(searchTerm) ||
                                        c.ClassId.Contains(searchTerm) ||
                                        c.Major.Contains(searchTerm) ||
                                        (c.AcademicYear != null && c.AcademicYear.Contains(searchTerm)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var classes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (classes, totalCount);
        }

        public async Task<IEnumerable<Class>> GetClassesByMajorAsync(string major)
        {
            return await _dbSet
                .Where(c => c.Major == major && c.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetClassesByAcademicYearAsync(string academicYear)
        {
            return await _dbSet
                .Where(c => c.AcademicYear == academicYear && c.IsActive)
                .ToListAsync();
        }

        public async Task<int> GetStudentCountInClassAsync(string classId)
        {
            return await _context.Students
                .CountAsync(s => s.ClassId == classId);
        }
        public async Task<bool> CanDeleteClassAsync(string classId)
        {

            var hasStudents = await _context.Students.AnyAsync(s => s.ClassId == classId);


            return !hasStudents;
        }
    }
}
