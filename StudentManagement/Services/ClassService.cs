using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Class;
using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;

        public ClassService(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync()
        {
            var classes = await _classRepository.GetAllAsync();
            var result = new List<ClassResponseDto>();

            foreach (var classItem in classes)
            {
                var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                result.Add(MapToResponseDto(classItem, studentCount));
            }

            return result;
        }

        public async Task<ClassResponseDto?> GetClassByIdAsync(string classId)
        {
            var classItem = await _classRepository.GetByIdAsync(classId);
            if (classItem == null)
                return null;

            var studentCount = await _classRepository.GetStudentCountInClassAsync(classId);
            return MapToResponseDto(classItem, studentCount);
        }

        public async Task<ClassWithStudentsDto?> GetClassWithStudentsAsync(string classId)
        {
            var classItem = await _classRepository.GetClassWithStudentsAsync(classId);
            if (classItem == null)
                return null;

            return MapToClassWithStudentsDto(classItem);
        }

        public async Task<ClassResponseDto> CreateClassAsync(CreateClassDto createClassDto)
        {
            // Check if ClassId already exists
            if (await _classRepository.IsClassIdExistsAsync(createClassDto.ClassId))
            {
                throw new InvalidOperationException("Class ID already exists");
            }

            // Check if ClassName already exists
            if (await _classRepository.IsClassNameExistsAsync(createClassDto.ClassName))
            {
                throw new InvalidOperationException("Class name already exists");
            }

            var classItem = MapToClass(createClassDto);
            var createdClass = await _classRepository.AddAsync(classItem);

            return MapToResponseDto(createdClass, 0);
        }

        public async Task<ClassResponseDto?> UpdateClassAsync(string classId, UpdateClassDto updateClassDto)
        {
            var classItem = await _classRepository.GetByIdAsync(classId);
            if (classItem == null)
                return null;

            // Check if ClassName already exists (excluding current class)
            if (await _classRepository.IsClassNameExistsAsync(updateClassDto.ClassName, classId))
            {
                throw new InvalidOperationException("Class name already exists");
            }

            // Update class properties
            classItem.ClassName = updateClassDto.ClassName;
            classItem.Major = updateClassDto.Major;
            classItem.AcademicYear = updateClassDto.AcademicYear;
            classItem.Semester = updateClassDto.Semester;
            classItem.IsActive = updateClassDto.IsActive;

            await _classRepository.UpdateAsync(classItem);

            var studentCount = await _classRepository.GetStudentCountInClassAsync(classId);
            return MapToResponseDto(classItem, studentCount);
        }

        public async Task<bool> DeleteClassAsync(string classId)
        {
            var classItem = await _classRepository.GetByIdAsync(classId);
            if (classItem == null)
                return false;

            // Check if class has students
            var studentCount = await _classRepository.GetStudentCountInClassAsync(classId);
            if (studentCount > 0)
            {
                throw new InvalidOperationException("Cannot delete class that has students enrolled");
            }

            await _classRepository.DeleteAsync(classItem);
            return true;
        }

        public async Task<IEnumerable<ClassResponseDto>> GetActiveClassesAsync()
        {
            var classes = await _classRepository.GetActiveClassesAsync();
            var result = new List<ClassResponseDto>();

            foreach (var classItem in classes)
            {
                var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                result.Add(MapToResponseDto(classItem, studentCount));
            }

            return result;
        }

        public async Task<IEnumerable<ClassResponseDto>> SearchClassesAsync(string searchTerm)
        {
            var classes = await _classRepository.SearchClassesAsync(searchTerm);
            var result = new List<ClassResponseDto>();

            foreach (var classItem in classes)
            {
                var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                result.Add(MapToResponseDto(classItem, studentCount));
            }

            return result;
        }

        public async Task<(IEnumerable<ClassResponseDto> Classes, int TotalCount)> GetPagedClassesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var (classes, totalCount) = await _classRepository.GetPagedClassesAsync(pageNumber, pageSize, searchTerm, isActive);
            var result = new List<ClassResponseDto>();

            foreach (var classItem in classes)
            {
                var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                result.Add(MapToResponseDto(classItem, studentCount));
            }

            return (result, totalCount);
        }

        public async Task<IEnumerable<ClassResponseDto>> GetClassesByMajorAsync(string major)
        {
            var classes = await _classRepository.GetClassesByMajorAsync(major);
            var result = new List<ClassResponseDto>();

            foreach (var classItem in classes)
            {
                var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                result.Add(MapToResponseDto(classItem, studentCount));
            }

            return result;
        }

        public async Task<IEnumerable<ClassResponseDto>> GetClassesByAcademicYearAsync(string academicYear)
        {
            var classes = await _classRepository.GetClassesByAcademicYearAsync(academicYear);
            var result = new List<ClassResponseDto>();

            foreach (var classItem in classes)
            {
                var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                result.Add(MapToResponseDto(classItem, studentCount));
            }

            return result;
        }

        public async Task<bool> IsClassIdExistsAsync(string classId)
        {
            return await _classRepository.IsClassIdExistsAsync(classId);
        }

        public async Task<bool> IsClassNameExistsAsync(string className, string? excludeClassId = null)
        {
            return await _classRepository.IsClassNameExistsAsync(className, excludeClassId);
        }

        public async Task<bool> CanDeleteClassAsync(string classId)
        {
            var studentCount = await _classRepository.GetStudentCountInClassAsync(classId);
            return studentCount == 0;
        }

        // Mapping methods
        private ClassResponseDto MapToResponseDto(Class classItem, int studentCount)
        {
            return new ClassResponseDto
            {
                ClassId = classItem.ClassId,
                ClassName = classItem.ClassName,
                Major = classItem.Major,
                AcademicYear = classItem.AcademicYear,
                Semester = classItem.Semester,
                IsActive = classItem.IsActive,
                StudentCount = studentCount
            };
        }

        private ClassWithStudentsDto MapToClassWithStudentsDto(Class classItem)
        {
            return new ClassWithStudentsDto
            {
                ClassId = classItem.ClassId,
                ClassName = classItem.ClassName,
                Major = classItem.Major,
                AcademicYear = classItem.AcademicYear,
                Semester = classItem.Semester,
                IsActive = classItem.IsActive,
                Students = classItem.Students.Select(MapStudentToResponseDto),
                StudentCount = classItem.Students.Count
            };
        }

        private Class MapToClass(CreateClassDto dto)
        {
            return new Class
            {
                ClassId = dto.ClassId,
                ClassName = dto.ClassName,
                Major = dto.Major,
                AcademicYear = dto.AcademicYear,
                Semester = dto.Semester,
                IsActive = dto.IsActive
            };
        }

        private StudentResponseDto MapStudentToResponseDto(Student student)
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
    }
}
