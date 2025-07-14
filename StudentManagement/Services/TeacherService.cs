using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Teacher;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<TeacherService> _logger;
        private const string CachePrefix = "teacher";

        public TeacherService(ITeacherRepository teacherRepository, ICacheService cacheService, ILogger<TeacherService> logger)
        {
            _teacherRepository = teacherRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<TeacherResponseDto?> GetTeacherByIdAsync(string teacherId)
        {
            _logger.LogInformation("Getting teacher by ID: {TeacherId}", teacherId);
            var cacheKey = $"{CachePrefix}:response:{teacherId}";

            var cachedData = await _cacheService.GetDataAsync<TeacherResponseDto>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Teacher {TeacherId} response found in cache.", teacherId);
                return cachedData;
            }

            var teacher = await _teacherRepository.GetTeacherWithUsersAsync(teacherId);
            if (teacher == null) return null;

            var dto = await MapToResponseDtoAsync(teacher);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(5));
            return dto;
        }

        public async Task<TeacherDetailResponseDto?> GetTeacherDetailByIdAsync(string teacherId)
        {
            _logger.LogInformation("Getting teacher detail by ID: {TeacherId}", teacherId);
            var cacheKey = $"{CachePrefix}:detail:{teacherId}";

            var cachedData = await _cacheService.GetDataAsync<TeacherDetailResponseDto>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("Teacher {TeacherId} detail found in cache.", teacherId);
                return cachedData;
            }

            var teacher = await _teacherRepository.GetTeacherFullDetailsAsync(teacherId);
            if (teacher == null) return null;

            var dto = MapToDetailResponseDto(teacher);
            await _cacheService.SetDataAsync(cacheKey, dto, System.DateTimeOffset.Now.AddMinutes(5));
            return dto;
        }

        public async Task<TeacherResponseDto> CreateTeacherAsync(CreateTeacherDto createTeacherDto)
        {
            _logger.LogInformation("Creating new teacher with ID: {TeacherId}", createTeacherDto.TeacherId);
            if (await _teacherRepository.IsTeacherIdExistsAsync(createTeacherDto.TeacherId) || await _teacherRepository.IsEmailExistsAsync(createTeacherDto.Email))
            {
                throw new InvalidOperationException("Teacher ID or Email already exists.");
            }

            var teacher = MapToTeacher(createTeacherDto);
            var createdTeacher = await _teacherRepository.AddAsync(teacher);
            _logger.LogInformation("Teacher {TeacherId} created successfully.", createdTeacher.TeacherId);

            await InvalidateTeacherListCaches();
            return await MapToResponseDtoAsync(createdTeacher);
        }

        public async Task<TeacherResponseDto?> UpdateTeacherAsync(string teacherId, UpdateTeacherDto updateTeacherDto)
        {
            _logger.LogInformation("Updating teacher with ID: {TeacherId}", teacherId);
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                 _logger.LogWarning("Update failed. Teacher with ID {TeacherId} not found.", teacherId);
                return null;
            }

            if (await _teacherRepository.IsEmailExistsAsync(updateTeacherDto.Email, teacherId))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            // SỬA LỖI: Cập nhật đầy đủ các thuộc tính
            teacher.FullName = updateTeacherDto.FullName;
            teacher.Email = updateTeacherDto.Email;
            teacher.PhoneNumber = updateTeacherDto.PhoneNumber;
            teacher.Department = updateTeacherDto.Department;
            teacher.Degree = updateTeacherDto.Degree;
            teacher.DateOfBirth = updateTeacherDto.DateOfBirth;
            teacher.HireDate = updateTeacherDto.HireDate;
            teacher.Salary = updateTeacherDto.Salary;

            await _teacherRepository.UpdateAsync(teacher);
            _logger.LogInformation("Teacher {TeacherId} updated successfully.", teacherId);

            // Invalidate caches
            await _cacheService.RemoveDataAsync($"{CachePrefix}:response:{teacherId}");
            await _cacheService.RemoveDataAsync($"{CachePrefix}:detail:{teacherId}");
            await InvalidateTeacherListCaches();
            _logger.LogInformation("Cache for teacher {TeacherId} and teacher lists invalidated.", teacherId);

            return await MapToResponseDtoAsync(teacher);
        }

        public async Task<bool> DeleteTeacherAsync(string teacherId)
        {
            _logger.LogInformation("Deleting teacher with ID: {TeacherId}", teacherId);
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null) return false;

            await _teacherRepository.DeleteAsync(teacher);
            _logger.LogInformation("Teacher {TeacherId} deleted successfully.", teacherId);

            // Invalidate caches
            await _cacheService.RemoveDataAsync($"{CachePrefix}:response:{teacherId}");
            await _cacheService.RemoveDataAsync($"{CachePrefix}:detail:{teacherId}");
            await InvalidateTeacherListCaches();
            _logger.LogInformation("Cache for teacher {TeacherId} and teacher lists invalidated.", teacherId);

            return true;
        }

        private Task InvalidateTeacherListCaches()
        {
            var tasks = new List<Task>
            {
                _cacheService.RemoveDataAsync($"{CachePrefix}es:distinct_departments"),
                _cacheService.RemoveDataAsync($"{CachePrefix}es:distinct_degrees")
            };
            return Task.WhenAll(tasks);
        }

        #region Other Public Methods

        public async Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync()
        {
            _logger.LogInformation("Getting all teachers.");
            var teachers = await _teacherRepository.GetAllAsync();
            var teacherDtos = new List<TeacherResponseDto>();
            foreach (var teacher in teachers)
            {
                teacherDtos.Add(await MapToResponseDtoAsync(teacher));
            }
            return teacherDtos;
        }

        public async Task<(IEnumerable<TeacherResponseDto> Teachers, int TotalCount)> GetPagedTeachersAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            _logger.LogInformation("Getting paged teachers. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
            var (teachers, totalCount) = await _teacherRepository.GetPagedTeachersAsync(pageNumber, pageSize, searchTerm);
            var teacherDtos = new List<TeacherResponseDto>();
            foreach (var teacher in teachers)
            {
                teacherDtos.Add(await MapToResponseDtoAsync(teacher));
            }
            return (teacherDtos, totalCount);
        }

        public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
        {
            const string cacheKey = $"{CachePrefix}es:distinct_departments";
            var cachedData = await _cacheService.GetDataAsync<IEnumerable<string>>(cacheKey);
            if(cachedData != null) return cachedData;

            var data = await _teacherRepository.GetDistinctDepartmentsAsync();
            await _cacheService.SetDataAsync(cacheKey, data, System.DateTimeOffset.Now.AddHours(1));
            return data;
        }

        public async Task<IEnumerable<string>> GetDistinctDegreesAsync()
        {
            const string cacheKey = $"{CachePrefix}es:distinct_degrees";
            var cachedData = await _cacheService.GetDataAsync<IEnumerable<string>>(cacheKey);
            if(cachedData != null) return cachedData;

            var data = await _teacherRepository.GetDistinctDegreesAsync();
            await _cacheService.SetDataAsync(cacheKey, data, System.DateTimeOffset.Now.AddHours(1));
            return data;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersByDepartmentAsync(string department)
        {
            _logger.LogInformation("Getting teachers by department: {Department}", department);
            var teachers = await _teacherRepository.GetTeachersByDepartmentAsync(department);
            var teacherDtos = new List<TeacherResponseDto>();
            foreach (var teacher in teachers)
            {
                teacherDtos.Add(await MapToResponseDtoAsync(teacher));
            }
            return teacherDtos;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersByDegreeAsync(string degree)
        {
            _logger.LogInformation("Getting teachers by degree: {Degree}", degree);
            var teachers = await _teacherRepository.GetTeachersByDegreeAsync(degree);
            var teacherDtos = new List<TeacherResponseDto>();
            foreach (var teacher in teachers)
            {
                teacherDtos.Add(await MapToResponseDtoAsync(teacher));
            }
            return teacherDtos;
        }

        public async Task<IEnumerable<TeacherResponseDto>> SearchTeachersAsync(string searchTerm)
        {
            _logger.LogInformation("Searching teachers with term: {SearchTerm}", searchTerm);
            var teachers = await _teacherRepository.SearchTeachersAsync(searchTerm);
            var teacherDtos = new List<TeacherResponseDto>();
            foreach (var teacher in teachers)
            {
                teacherDtos.Add(await MapToResponseDtoAsync(teacher));
            }
            return teacherDtos;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersBySalaryRangeAsync(decimal? minSalary, decimal? maxSalary)
        {
            _logger.LogInformation("Getting teachers by salary range.");
            var teachers = await _teacherRepository.GetTeachersBySalaryRangeAsync(minSalary, maxSalary);
            var teacherDtos = new List<TeacherResponseDto>();
            foreach (var teacher in teachers)
            {
                teacherDtos.Add(await MapToResponseDtoAsync(teacher));
            }
            return teacherDtos;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetTeachersWithActiveCoursesAsync()
        {
            _logger.LogInformation("Getting teachers with active courses.");
            var teachers = await _teacherRepository.GetTeachersWithActiveCourses();
            var teacherDtos = new List<TeacherResponseDto>();
            foreach (var teacher in teachers)
            {
                teacherDtos.Add(await MapToResponseDtoAsync(teacher));
            }
            return teacherDtos;
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeTeacherId = null) => await _teacherRepository.IsEmailExistsAsync(email, excludeTeacherId);
        public async Task<bool> IsTeacherIdExistsAsync(string teacherId) => await _teacherRepository.IsTeacherIdExistsAsync(teacherId);

        #endregion

        #region Mapping Methods
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
        #endregion
    }
}
