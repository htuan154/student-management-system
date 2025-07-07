using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync()
        {
            var students = await _studentRepository.GetAllAsync();
            return students.Select(MapToResponseDto);
        }

        public async Task<StudentResponseDto?> GetStudentByIdAsync(string studentId)
        {
            var student = await _studentRepository.GetStudentWithClassAsync(studentId);
            return student == null ? null : MapToResponseDto(student);
        }

        public async Task<StudentResponseDto> CreateStudentAsync(CreateStudentDto createStudentDto)
        {
            // Check if StudentId already exists
            if (await _studentRepository.IsStudentIdExistsAsync(createStudentDto.StudentId))
            {
                throw new InvalidOperationException("Student ID already exists");
            }


            if (await _studentRepository.IsEmailExistsAsync(createStudentDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var student = MapToStudent(createStudentDto);
            var createdStudent = await _studentRepository.AddAsync(student);


            var studentWithClass = await _studentRepository.GetStudentWithClassAsync(createdStudent.StudentId);
            return MapToResponseDto(studentWithClass!);
        }

        public async Task<StudentResponseDto?> UpdateStudentAsync(string studentId, UpdateStudentDto updateStudentDto)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return null;


            if (await _studentRepository.IsEmailExistsAsync(updateStudentDto.Email, studentId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Update student properties
            student.FullName = updateStudentDto.FullName;
            student.Email = updateStudentDto.Email;
            student.PhoneNumber = updateStudentDto.PhoneNumber;
            student.DateOfBirth = updateStudentDto.DateOfBirth;
            student.Address = updateStudentDto.Address;
            student.ClassId = updateStudentDto.ClassId;

            await _studentRepository.UpdateAsync(student);

            // Get updated student with class information
            var updatedStudent = await _studentRepository.GetStudentWithClassAsync(studentId);
            return MapToResponseDto(updatedStudent!);
        }

        public async Task<bool> DeleteStudentAsync(string studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return false;

            await _studentRepository.DeleteAsync(student);
            return true;
        }

        public async Task<IEnumerable<StudentResponseDto>> GetStudentsByClassIdAsync(string classId)
        {
            var students = await _studentRepository.GetStudentsByClassIdAsync(classId);
            return students.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<StudentResponseDto>> SearchStudentsAsync(string searchTerm)
        {
            var students = await _studentRepository.SearchStudentsAsync(searchTerm);
            return students.Select(MapToResponseDto);
        }

        public async Task<(IEnumerable<StudentResponseDto> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
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
