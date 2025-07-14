using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            IStudentRepository studentRepository,
            ICacheService cacheService,
            ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync()
        {
            _logger.LogInformation("Attempting to get all students.");

            var students = await _studentRepository.GetAllAsync();
            return students.Select(MapToResponseDto);
        }

        public async Task<StudentResponseDto?> GetStudentByIdAsync(string studentId)
        {
            _logger.LogInformation("Attempting to get student with ID: {StudentId}", studentId);

            // 1. Define a unique cache key
            var cacheKey = $"student:{studentId}";

            // 2. Try to get data from cache
            var cachedStudent = await _cacheService.GetDataAsync<StudentResponseDto>(cacheKey);
            if (cachedStudent != null)
            {
                _logger.LogInformation("Student with ID {StudentId} found in cache.", studentId);
                return cachedStudent;
            }

            _logger.LogInformation("Student with ID {StudentId} not found in cache. Fetching from database.", studentId);

            // 3. If not in cache, get from database
            var student = await _studentRepository.GetStudentWithClassAsync(studentId);
            if (student == null) return null;

            var studentDto = MapToResponseDto(student);

            // 4. Set data to cache for next time
            var expirationTime = DateTimeOffset.Now.AddMinutes(5);
            await _cacheService.SetDataAsync(cacheKey, studentDto, expirationTime);

            return studentDto;
        }

        public async Task<StudentResponseDto> CreateStudentAsync(CreateStudentDto createStudentDto)
        {
            _logger.LogInformation("Attempting to create a new student with ID: {StudentId}", createStudentDto.StudentId);

            if (await _studentRepository.IsStudentIdExistsAsync(createStudentDto.StudentId))
            {
                _logger.LogWarning("Create failed: Student ID {StudentId} already exists.", createStudentDto.StudentId);
                throw new InvalidOperationException("Student ID already exists");
            }

            if (await _studentRepository.IsEmailExistsAsync(createStudentDto.Email))
            {
                _logger.LogWarning("Create failed: Email {Email} already exists.", createStudentDto.Email);
                throw new InvalidOperationException("Email already exists");
            }

            var student = MapToStudent(createStudentDto);
            var createdStudent = await _studentRepository.AddAsync(student);

            // Invalidate any cached lists of students, as they are now outdated
            await _cacheService.RemoveDataAsync("paged_students:*"); // Example for removing paged results

            _logger.LogInformation("Student with ID {StudentId} created successfully.", createdStudent.StudentId);

            var studentWithClass = await _studentRepository.GetStudentWithClassAsync(createdStudent.StudentId);
            return MapToResponseDto(studentWithClass!);
        }

        public async Task<StudentResponseDto?> UpdateStudentAsync(string studentId, UpdateStudentDto updateStudentDto)
        {
            _logger.LogInformation("Attempting to update student with ID: {StudentId}", studentId);

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning("Update failed: Student with ID {StudentId} not found.", studentId);
                return null;
            }

            if (await _studentRepository.IsEmailExistsAsync(updateStudentDto.Email, studentId))
            {
                _logger.LogWarning("Update failed: Email {Email} already exists.", updateStudentDto.Email);
                throw new InvalidOperationException("Email already exists");
            }

            student.FullName = updateStudentDto.FullName;
            student.Email = updateStudentDto.Email;
            student.PhoneNumber = updateStudentDto.PhoneNumber;
            student.DateOfBirth = updateStudentDto.DateOfBirth;
            student.Address = updateStudentDto.Address;
            student.ClassId = updateStudentDto.ClassId;

            await _studentRepository.UpdateAsync(student);

            // Remove the specific student's cache entry as it is now outdated
            var cacheKey = $"student:{studentId}";
            await _cacheService.RemoveDataAsync(cacheKey);

            _logger.LogInformation("Student with ID {StudentId} updated successfully and cache was invalidated.", studentId);

            var updatedStudent = await _studentRepository.GetStudentWithClassAsync(studentId);
            return MapToResponseDto(updatedStudent!);
        }

        public async Task<bool> DeleteStudentAsync(string studentId)
        {
            _logger.LogInformation("Attempting to delete student with ID: {StudentId}", studentId);

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning("Delete failed: Student with ID {StudentId} not found.", studentId);
                return false;
            }

            await _studentRepository.DeleteAsync(student);

            // Remove the specific student's cache entry
            var cacheKey = $"student:{studentId}";
            await _cacheService.RemoveDataAsync(cacheKey);

            _logger.LogInformation("Student with ID {StudentId} deleted successfully and cache was invalidated.", studentId);

            return true;
        }

        // Other methods remain largely the same, but can also have logging/caching added if needed

        public async Task<IEnumerable<StudentResponseDto>> GetStudentsByClassIdAsync(string classId)
        {
            _logger.LogInformation("Getting students for class ID: {ClassId}", classId);
            var students = await _studentRepository.GetStudentsByClassIdAsync(classId);
            return students.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<StudentResponseDto>> SearchStudentsAsync(string searchTerm)
        {
            _logger.LogInformation("Searching for students with term: {SearchTerm}", searchTerm);
            var students = await _studentRepository.SearchStudentsAsync(searchTerm);
            return students.Select(MapToResponseDto);
        }

        public async Task<(IEnumerable<StudentResponseDto> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            _logger.LogInformation("Getting paged students. Page: {PageNumber}, Size: {PageSize}, Term: {SearchTerm}", pageNumber, pageSize, searchTerm);
            var (students, totalCount) = await _studentRepository.GetPagedStudentsAsync(pageNumber, pageSize, searchTerm);
            var studentDtos = students.Select(MapToResponseDto);
            return (studentDtos, totalCount);
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeStudentId = null)
        {
            return await _studentRepository.IsEmailExistsAsync(email, excludeStudentId);
        }

        public async Task<bool> IsStudentIdExistsAsync(string studentId)
        {
            return await _studentRepository.IsStudentIdExistsAsync(studentId);
        }


        private StudentResponseDto MapToResponseDto(Student student)
        {
            return new StudentResponseDto
            {
                StudentId = student.StudentId,
                FullName = student.FullName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                Address = student.Address,
                ClassId = student.ClassId,
                ClassName = student.Class?.ClassName ?? string.Empty
            };
        }

        private Student MapToStudent(CreateStudentDto dto)
        {
            return new Student
            {
                StudentId = dto.StudentId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                ClassId = dto.ClassId
            };
        }
    }
}
