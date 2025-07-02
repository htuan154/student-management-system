using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.Dtos.Enrollment;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public EnrollmentService(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<EnrollmentDto?> GetByIdAsync(int enrollmentId)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null) return null;
            return MapToDto(enrollment);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetByStudentIdAsync(string studentId)
        {
            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
            return enrollments.Select(MapToDto);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetByCourseIdAsync(string courseId)
        {
            var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId);
            return enrollments.Select(MapToDto);
        }

        public async Task<IEnumerable<EnrollmentDto>> SearchAsync(string searchTerm)
        {
            var enrollments = await _enrollmentRepository.SearchAsync(searchTerm);
            return enrollments.Select(MapToDto);
        }

        public async Task<(IEnumerable<EnrollmentDto> Enrollments, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (enrollments, totalCount) = await _enrollmentRepository.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return (enrollments.Select(MapToDto), totalCount);
        }

        public async Task<bool> CreateAsync(EnrollmentCreateDto dto)
        {
            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId,
                Semester = dto.Semester,
                Year = dto.Year,
                Status = dto.Status
            };
            await _enrollmentRepository.AddAsync(enrollment);
            return true;
        }

        public async Task<bool> UpdateAsync(EnrollmentUpdateDto dto)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(dto.EnrollmentId);
            if (enrollment == null) return false;
            enrollment.StudentId = dto.StudentId;
            enrollment.CourseId = dto.CourseId;
            enrollment.TeacherId = dto.TeacherId;
            enrollment.Semester = dto.Semester;
            enrollment.Year = dto.Year;
            enrollment.Status = dto.Status;
            await _enrollmentRepository.UpdateAsync(enrollment);
            return true;
        }

        public async Task<bool> DeleteAsync(int enrollmentId)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null) return false;
            await _enrollmentRepository.DeleteAsync(enrollment);
            return true;
        }

        private static EnrollmentDto MapToDto(Enrollment e) => new EnrollmentDto
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            CourseId = e.CourseId,
            TeacherId = e.TeacherId,
            Semester = e.Semester,
            Year = e.Year,
            Status = e.Status
        };
    }
}