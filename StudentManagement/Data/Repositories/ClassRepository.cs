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
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(c => c.ClassId == classId);
        }


        public async Task<Class?> GetClassWithFullDetailsAsync(string classId)
        {
            return await _dbSet
                .Include(c => c.Students)
                .Include(c => c.Semester)
                .Include(c => c.AnnouncementDetails) // Nếu cần thông báo
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
                .Include(c => c.Semester)
                .Where(c => c.ClassName.Contains(searchTerm) ||
                           c.ClassId.Contains(searchTerm) ||
                           c.Major.Contains(searchTerm) ||
                           (c.Semester != null && c.Semester.SemesterName.Contains(searchTerm)) || // ✅ THÊM
                           (c.Semester != null && c.Semester.AcademicYear.Contains(searchTerm))) // ✅ THÊM
                .ToListAsync();
        }

        public async Task<(IEnumerable<Class> Classes, int TotalCount)> GetPagedClassesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null, int? semesterId = null) // ✅ THÊM semesterId
        {
            var query = _dbSet
                .Include(c => c.Semester) // ✅ THÊM
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.ClassName.Contains(searchTerm) ||
                                        c.ClassId.Contains(searchTerm) ||
                                        c.Major.Contains(searchTerm) ||
                                        (c.Semester != null && c.Semester.SemesterName.Contains(searchTerm)) || // ✅ THÊM
                                        (c.Semester != null && c.Semester.AcademicYear.Contains(searchTerm))); // ✅ THÊM
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // ✅ THÊM - Filter by semester
            if (semesterId.HasValue)
            {
                query = query.Where(c => c.SemesterId == semesterId.Value);
            }

            var totalCount = await query.CountAsync();
            var classes = await query
                .OrderBy(c => c.ClassName) // ✅ THÊM ordering
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
                .Include(c => c.Semester)
                .Where(c => c.Semester != null && c.Semester.AcademicYear == academicYear && c.IsActive)
                .OrderBy(c => c.ClassName)
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
            var hasAnnouncements = await _context.AnnouncementDetails.AnyAsync(ad => ad.ClassId == classId);

            return !hasStudents && !hasAnnouncements; // ✅ Check cả announcements
        }

        // ✅ THÊM - Statistics methods
        public async Task<int> GetClassCountBySemesterAsync(int semesterId)
        {
            return await _dbSet
                .CountAsync(c => c.SemesterId == semesterId);
        }

        public async Task<int> GetActiveClassCountBySemesterAsync(int semesterId)
        {
            return await _dbSet
                .CountAsync(c => c.SemesterId == semesterId && c.IsActive);
        }


        public async Task<IEnumerable<Class>> GetClassesBySemesterIdAsync(int semesterId)
        {
            return await _dbSet
                .Include(c => c.Semester)
                .Where(c => c.SemesterId == semesterId)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Class>> GetActiveClassesBySemesterIdAsync(int semesterId)
        {
            return await _dbSet
                .Include(c => c.Semester)
                .Where(c => c.SemesterId == semesterId && c.IsActive)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Class>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Semester)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }


        public override async Task<Class?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(c => c.Semester)
                .FirstOrDefaultAsync(c => c.ClassId == (string)id);
        }
    }
}
