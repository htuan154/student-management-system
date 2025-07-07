using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Teacher;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teacherRepository;

        public TeacherService(ITeacherRepository teacherRepository)
        {
            _teacherRepository = teacherRepository;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync()
        {
            var teachers = await _teacherRepository.GetAllAsync();
            var teacherDtos = new List<TeacherResponseDto>();

            foreach (var teacher in teachers)
            {
                var dto = await MapToResponseDtoAsync(teacher);
                teacherDtos.Add(dto);
            }

            return teacherDtos;
        }

        public async Task<TeacherResponseDto?> GetTeacherByIdAsync(string teacherId)
        {
            var teacher = await _teacherRepository.GetTeacherWithUsersAsync(teacherId);
            if (teacher == null) return null;

            return await MapToResponseDtoAsync(teacher);
        }

        public async Task<TeacherDetailResponseDto?> GetTeacherDetailByIdAsync(string teacherId)
        {
            var teacher = await _teacherRepository.GetTeacherFullDetailsAsync(teacherId);
            if (teacher == null) return null;

            return MapToDetailResponseDto(teacher);
        }

        public async Task<TeacherResponseDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto)
        {

            if (await _teacherRepository.IsTeacherIdExistsAsync(createTeacherDto.TeacherId))
            {
                throw new InvalidOperationException("Teacher ID already exists");
            }

            // Check if Email already exists
            if (await _teacherRepository.IsEmailExistsAsync(createTeacherDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var teacher = MapToTeacher(createTeacherDto);
            var createdTeacher = await _teacherRepository.AddAsync(teacher);

            return await MapToResponseDtoAsync(createdTeacher);
        }

        public async Task<TeacherResponseDto?> UpdateTeacherAsync(string teacherId, UpdateTeacherDto updateTeacherDto)
        {
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
                return null;


            if (await _teacherRepository.IsEmailExistsAsync(updateTeacherDto.Email, teacherId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Update teacher properties
            teacher.FullName = updateTeacherDto.FullName;
            teacher.Email = updateTeacherDto.Email;
            teacher.PhoneNumber = updateTeacherDto.PhoneNumber;
            teacher.Department = updateTeacherDto.Department;
            teacher.Degree = updateTeacherDto.Degree;
            teacher.DateOfBirth = updateTeacherDto.DateOfBirth;
            teacher.HireDate = updateTeacherDto.HireDate;
            teacher.Salary = updateTeacherDto.Salary;

            await _teacherRepository.UpdateAsync(teacher);

            return await MapToResponseDtoAsync(teacher);
        }

        public async Task<bool> DeleteTeacherAsync(string teacherId)
        {
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
                return false;

            await _teacherRepository.DeleteAsync(teacher);
            return true;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersByDepartmentAsync(string department)
        {
            var teachers = await _teacherRepository.GetTeachersByDepartmentAsync(department);
            var teacherDtos = new List<TeacherResponseDto>();

            foreach (var teacher in teachers)
            {
                var dto = await MapToResponseDtoAsync(teacher);
                teacherDtos.Add(dto);
            }

            return teacherDtos;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersByDegreeAsync(string degree)
        {
            var teachers = await _teacherRepository.GetTeachersByDegreeAsync(degree);
            var teacherDtos = new List<TeacherResponseDto>();

            foreach (var teacher in teachers)
            {
                var dto = await MapToResponseDtoAsync(teacher);
                teacherDtos.Add(dto);
            }

            return teacherDtos;
        }

        public async Task<IEnumerable<TeacherResponseDto>> SearchTeachersAsync(string searchTerm)
        {
            var teachers = await _teacherRepository.SearchTeachersAsync(searchTerm);
            var teacherDtos = new List<TeacherResponseDto>();

            foreach (var teacher in teachers)
            {
                var dto = await MapToResponseDtoAsync(teacher);
                teacherDtos.Add(dto);
            }

            return teacherDtos;
        }

        public async Task<(IEnumerable<TeacherResponseDto> Teachers, int TotalCount)> GetPagedTeachersAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var (teachers, totalCount) = await _teacherRepository.GetPagedTeachersAsync(pageNumber, pageSize, searchTerm);
            var teacherDtos = new List<TeacherResponseDto>();

            foreach (var teacher in teachers)
            {
                var dto = await MapToResponseDtoAsync(teacher);
                teacherDtos.Add(dto);
            }

            return (teacherDtos, totalCount);
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary)
        {
            var teachers = await _teacherRepository.GetTeachersBySalaryRangeAsync(minSalary, maxSalary);
            var teacherDtos = new List<TeacherResponseDto>();

            foreach (var teacher in teachers)
            {
                var dto = await MapToResponseDtoAsync(teacher);
                teacherDtos.Add(dto);
            }

            return teacherDtos;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersWithActiveCoursesAsync()
        {
            var teachers = await _teacherRepository.GetTeachersWithActiveCourses();
            var teacherDtos = new List<TeacherResponseDto>();

            foreach (var teacher in teachers)
            {
                var dto = await MapToResponseDtoAsync(teacher);
                teacherDtos.Add(dto);
            }

            return teacherDtos;
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeTeacherId = null)
        {
            return await _teacherRepository.IsEmailExistsAsync(email, excludeTeacherId);
        }

        public async Task<bool> IsTeacherIdExistsAsync(string teacherId)
        {
            return await _teacherRepository.IsTeacherIdExistsAsync(teacherId);
        }

        public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
        {
            return await _teacherRepository.GetDistinctDepartmentsAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctDegreesAsync()
        {
            return await _teacherRepository.GetDistinctDegreesAsync();
        }

        
        private async Task<TeacherResponseDto> MapToResponseDtoAsync(Teacher teacher)
        {
            var courseCount = await _teacherRepository.GetTeacherCourseCountAsync(teacher.TeacherId);
            var enrollmentCount = await _teacherRepository.GetTeacherEnrollmentCountAsync(teacher.TeacherId);

            return new TeacherResponseDto
            {
                TeacherId = teacher.TeacherId,
                FullName = teacher.FullName,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                Department = teacher.Department,
                Degree = teacher.Degree,
                DateOfBirth = teacher.DateOfBirth,
                HireDate = teacher.HireDate,
                Salary = teacher.Salary,
                UserCount = teacher.Users?.Count ?? 0,
                CourseCount = courseCount,
                EnrollmentCount = enrollmentCount
            };
        }

        private TeacherDetailResponseDto MapToDetailResponseDto(Teacher teacher)
        {
            return new TeacherDetailResponseDto
            {
                TeacherId = teacher.TeacherId,
                FullName = teacher.FullName,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                Department = teacher.Department,
                Degree = teacher.Degree,
                DateOfBirth = teacher.DateOfBirth,
                HireDate = teacher.HireDate,
                Salary = teacher.Salary,
                Courses = teacher.TeacherCourses?.Select(tc => new CourseInfoDto
                {
                    CourseId = tc.Course.CourseId,
                    CourseName = tc.Course.CourseName,
                    Credits = tc.Course.Credits
                }).ToList() ?? new List<CourseInfoDto>(),
                Enrollments = teacher.Enrollments?.Select(e => new EnrollmentInfoDto
                {
                    StudentId = e.Student.StudentId,
                    StudentName = e.Student.FullName,
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName,
                    Status = e.Status
                }).ToList() ?? new List<EnrollmentInfoDto>()
            };
        }

        private Teacher MapToTeacher(CreateTeacherDto dto)
        {
            return new Teacher
            {
                TeacherId = dto.TeacherId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Department = dto.Department,
                Degree = dto.Degree,
                DateOfBirth = dto.DateOfBirth,
                HireDate = dto.HireDate,
                Salary = dto.Salary
            };
        }
    }
}
