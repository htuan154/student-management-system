using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.TeacherCourse;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class TeacherCourseService : ITeacherCourseService
    {
        private readonly ITeacherCourseRepository _teacherCourseRepository;

        public TeacherCourseService(ITeacherCourseRepository teacherCourseRepository)
        {
            _teacherCourseRepository = teacherCourseRepository;
        }

        public async Task<TeacherCourseDto?> GetByIdAsync(int teacherCourseId)
        {
            var tc = await _teacherCourseRepository.GetByIdAsync(teacherCourseId);
            return tc == null ? null : MapToDto(tc);
        }

        public async Task<IEnumerable<TeacherCourseDto>> GetByTeacherIdAsync(string teacherId)
        {
            var tcs = await _teacherCourseRepository.GetByTeacherIdAsync(teacherId);
            return tcs.Select(MapToDto);
        }

        public async Task<IEnumerable<TeacherCourseDto>> GetByCourseIdAsync(string courseId)
        {
            var tcs = await _teacherCourseRepository.GetByCourseIdAsync(courseId);
            return tcs.Select(MapToDto);
        }

        public async Task<IEnumerable<TeacherCourseDto>> SearchAsync(string searchTerm)
        {
            var tcs = await _teacherCourseRepository.SearchAsync(searchTerm);
            return tcs.Select(MapToDto);
        }

        public async Task<(IEnumerable<TeacherCourseDto> TeacherCourses, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (tcs, totalCount) = await _teacherCourseRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return (tcs.Select(MapToDto), totalCount);
        }

        public async Task<bool> CreateAsync(TeacherCourseCreateDto dto)
        {
            var tc = new TeacherCourse
            {
                TeacherId = dto.TeacherId,
                CourseId = dto.CourseId,
                Semester = dto.Semester,
                Year = dto.Year,
                IsActive = dto.IsActive
            };
            await _teacherCourseRepository.AddAsync(tc);
            return true;
        }

        public async Task<bool> UpdateAsync(TeacherCourseUpdateDto dto)
        {
            var tc = await _teacherCourseRepository.GetByIdAsync(dto.TeacherCourseId);
            if (tc == null) return false;
            tc.TeacherId = dto.TeacherId;
            tc.CourseId = dto.CourseId;
            tc.Semester = dto.Semester;
            tc.Year = dto.Year;
            tc.IsActive = dto.IsActive;
            await _teacherCourseRepository.UpdateAsync(tc);
            return true;
        }

        public async Task<bool> DeleteAsync(int teacherCourseId)
        {
            var tc = await _teacherCourseRepository.GetByIdAsync(teacherCourseId);
            if (tc == null) return false;
            await _teacherCourseRepository.DeleteAsync(tc);
            return true;
        }

        private static TeacherCourseDto MapToDto(TeacherCourse tc) => new TeacherCourseDto
        {
            TeacherCourseId = tc.TeacherCourseId,
            TeacherId = tc.TeacherId,
            CourseId = tc.CourseId,
            Semester = tc.Semester,
            Year = tc.Year,
            IsActive = tc.IsActive
        };
    }
}