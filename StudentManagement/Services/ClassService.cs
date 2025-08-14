
using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Class;
using StudentManagementSystem.DTOs.Student;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagementSystem.Services
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ClassService> _logger;
        private const string ClassCacheKeyPrefix = "class";

        public ClassService(IClassRepository classRepository, ICacheService cacheService, ILogger<ClassService> logger)
        {
            _classRepository = classRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync()
        {
            _logger.LogInformation("Attempting to get all classes.");
            try
            {
                _logger.LogWarning("Executing GetAllClassesAsync which has a potential N+1 query performance issue.");
                var classes = await _classRepository.GetAllAsync();
                var result = new List<ClassResponseDto>();

                foreach (var classItem in classes)
                {
                    var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                    result.Add(MapToResponseDto(classItem, studentCount));
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all classes.");
                throw;
            }
        }

        public async Task<ClassResponseDto?> GetClassByIdAsync(string classId)
        {
            _logger.LogInformation("Attempting to get class by ID: {ClassId}", classId);
            try
            {
                var cacheKey = $"{ClassCacheKeyPrefix}:{classId}";
                var cachedClass = await _cacheService.GetDataAsync<ClassResponseDto>(cacheKey);
                if (cachedClass != null)
                {
                    _logger.LogInformation("Class {ClassId} found in cache.", classId);
                    return cachedClass;
                }

                _logger.LogInformation("Class {ClassId} not in cache. Fetching from database.", classId);
                var classItem = await _classRepository.GetByIdAsync(classId);
                if (classItem == null)
                {
                    _logger.LogWarning("Class with ID {ClassId} not found.", classId);
                    return null;
                }

                var studentCount = await _classRepository.GetStudentCountInClassAsync(classId);
                var classDto = MapToResponseDto(classItem, studentCount);

                await _cacheService.SetDataAsync(cacheKey, classDto, DateTimeOffset.Now.AddMinutes(5));
                return classDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting class by ID {ClassId}.", classId);
                throw;
            }
        }

        public async Task<ClassWithStudentsDto?> GetClassWithStudentsAsync(string classId)
        {
            _logger.LogInformation("Attempting to get class with students by ID: {ClassId}", classId);
            try
            {
                var cacheKey = $"{ClassCacheKeyPrefix}:{classId}:students";
                var cachedData = await _cacheService.GetDataAsync<ClassWithStudentsDto>(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Class with students {ClassId} found in cache.", classId);
                    return cachedData;
                }

                var classItem = await _classRepository.GetClassWithStudentsAsync(classId);
                if (classItem == null)
                {
                    _logger.LogWarning("Class with ID {ClassId} not found when trying to get with students.", classId);
                    return null;
                }

                var classDto = MapToClassWithStudentsDto(classItem);
                await _cacheService.SetDataAsync(cacheKey, classDto, DateTimeOffset.Now.AddMinutes(5));
                return classDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting class {ClassId} with students.", classId);
                throw;
            }
        }

        public async Task<ClassResponseDto> CreateClassAsync(CreateClassDto createClassDto)
        {
            _logger.LogInformation("Attempting to create new class with ID: {ClassId}", createClassDto.ClassId);
            try
            {
                if (await _classRepository.IsClassIdExistsAsync(createClassDto.ClassId) ||
                    await _classRepository.IsClassNameExistsAsync(createClassDto.ClassName))
                {
                    _logger.LogWarning("Attempted to create class with existing ID {ClassId} or Name {ClassName}.", createClassDto.ClassId, createClassDto.ClassName);
                    throw new InvalidOperationException("Class ID or name already exists.");
                }

                var classItem = MapToClass(createClassDto);
                var createdClass = await _classRepository.AddAsync(classItem);

                _logger.LogInformation("Class {ClassId} created successfully.", createdClass.ClassId);
                return MapToResponseDto(createdClass, 0);
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating class {ClassId}.", createClassDto.ClassId);
                throw;
            }
        }

        public async Task<ClassResponseDto?> UpdateClassAsync(string classId, UpdateClassDto updateClassDto)
        {
            _logger.LogInformation("Attempting to update class with ID: {ClassId}", classId);
            try
            {
                var classItem = await _classRepository.GetByIdAsync(classId);
                if (classItem == null)
                {
                    _logger.LogWarning("Update failed. Class with ID {ClassId} not found.", classId);
                    return null;
                }

                if (await _classRepository.IsClassNameExistsAsync(updateClassDto.ClassName, classId))
                {
                    _logger.LogWarning("Update failed. Class name '{ClassName}' already exists for a different class.", updateClassDto.ClassName);
                    throw new InvalidOperationException("Class name already exists.");
                }


                classItem.ClassName = updateClassDto.ClassName;
                classItem.Major = updateClassDto.Major;
                classItem.SemesterId = updateClassDto.SemesterId;
                classItem.IsActive = updateClassDto.IsActive;

                await _classRepository.UpdateAsync(classItem);
                await InvalidateClassCacheAsync(classId);
                var studentCount = await _classRepository.GetStudentCountInClassAsync(classId);
                return MapToResponseDto(classItem, studentCount);
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating class {ClassId}.", classId);
                throw;
            }
        }

        public async Task<bool> DeleteClassAsync(string classId)
        {
            _logger.LogInformation("Attempting to delete class with ID: {ClassId}", classId);
            try
            {
                var classItem = await _classRepository.GetByIdAsync(classId);
                if (classItem == null)
                {
                    _logger.LogWarning("Delete failed. Class with ID {ClassId} not found.", classId);
                    return false;
                }

                if (await _classRepository.CanDeleteClassAsync(classId) == false)
                {
                    _logger.LogWarning("Delete failed. Class {ClassId} has students enrolled.", classId);
                    throw new InvalidOperationException("Cannot delete class that has students enrolled.");
                }

                await _classRepository.DeleteAsync(classItem);
                await InvalidateClassCacheAsync(classId);

                _logger.LogInformation("Class {ClassId} deleted successfully and cache invalidated.", classId);
                return true;
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting class {ClassId}.", classId);
                throw;
            }
        }

        public async Task<IEnumerable<ClassResponseDto>> GetActiveClassesAsync()
        {
            _logger.LogInformation("Attempting to get all active classes.");
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting active classes.");
                throw;
            }
        }

        public async Task<IEnumerable<ClassResponseDto>> SearchClassesAsync(string searchTerm)
        {
            _logger.LogInformation("Attempting to search classes with term: {SearchTerm}", searchTerm);
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching classes with term {SearchTerm}.", searchTerm);
                throw;
            }
        }

        public async Task<(IEnumerable<ClassResponseDto> Classes, int TotalCount)> GetPagedClassesAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null, int? semesterId = null)
        {
            _logger.LogInformation("Attempting to get paged classes. Page: {PageNumber}, Size: {PageSize}, Term: {SearchTerm}, IsActive: {IsActive}, SemesterId: {SemesterId}",
                pageNumber, pageSize, searchTerm, isActive, semesterId);
            try
            {
                var (classes, totalCount) = await _classRepository.GetPagedClassesAsync(pageNumber, pageSize, searchTerm, isActive, semesterId);
                var result = new List<ClassResponseDto>();
                foreach (var classItem in classes)
                {
                    var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                    result.Add(MapToResponseDto(classItem, studentCount));
                }
                return (result, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting paged classes.");
                throw;
            }
        }

        public async Task<IEnumerable<ClassResponseDto>> GetClassesByMajorAsync(string major)
        {
            _logger.LogInformation("Attempting to get classes by major: {Major}", major);
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting classes by major {Major}.", major);
                throw;
            }
        }

        public async Task<IEnumerable<ClassResponseDto>> GetClassesBySemesterIdAsync(int semesterId)
        {
            _logger.LogInformation("Attempting to get classes by semester ID: {SemesterId}", semesterId);
            try
            {
                var classes = await _classRepository.GetClassesBySemesterIdAsync(semesterId);
                var result = new List<ClassResponseDto>();
                foreach (var classItem in classes)
                {
                    var studentCount = await _classRepository.GetStudentCountInClassAsync(classItem.ClassId);
                    result.Add(MapToResponseDto(classItem, studentCount));
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting classes by semester ID {SemesterId}.", semesterId);
                throw;
            }
        }

        public async Task<IEnumerable<ClassResponseDto>> GetClassesByAcademicYearAsync(string academicYear)
        {
            _logger.LogInformation("Attempting to get classes by academic year: {AcademicYear}", academicYear);
            try
            {
                var classes = await _classRepository.GetClassesByAcademicYearAsync(academicYear);
                var result = new List<ClassResponseDto>();
                foreach (var classItem in classes)
                {
                    var studentCount = await _classRepository.GetStudentCountInClassAsync (classItem.ClassId);
                    result.Add(MapToResponseDto(classItem, studentCount));
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting classes by academic year {AcademicYear}.", academicYear);
                throw;
            }
        }

        public async Task<bool> IsClassIdExistsAsync(string classId)
        {
            _logger.LogInformation("Checking if Class ID exists: {ClassId}", classId);
            try
            {
                return await _classRepository.IsClassIdExistsAsync(classId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for Class ID {ClassId}.", classId);
                throw;
            }
        }

        public async Task<bool> IsClassNameExistsAsync(string className, string? excludeClassId = null)
        {
            _logger.LogInformation("Checking if Class Name exists: {ClassName}, excluding ID: {ExcludeClassId}", className, excludeClassId);
            try
            {
                return await _classRepository.IsClassNameExistsAsync(className, excludeClassId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for Class Name {ClassName}.", className);
                throw;
            }
        }

        public async Task<bool> CanDeleteClassAsync(string classId)
        {
            _logger.LogInformation("Checking if class can be deleted: {ClassId}", classId);
            try
            {
                return await _classRepository.CanDeleteClassAsync(classId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking if class {ClassId} can be deleted.", classId);
                throw;
            }
        }

        #region Private Helper and Mapping Methods

        // ✅ THÊM - InvalidateClassCacheAsync method
        private async Task InvalidateClassCacheAsync(string classId)
        {
            var cacheKeys = new[]
            {
                $"{ClassCacheKeyPrefix}:{classId}",
                $"{ClassCacheKeyPrefix}:{classId}:students"
            };

            foreach (var key in cacheKeys)
            {
                await _cacheService.RemoveDataAsync(key);
            }

            _logger.LogInformation("Cache invalidated for class {ClassId}.", classId);
        }

        // ✅ SỬA - MapToResponseDto sử dụng Semester navigation property
        private ClassResponseDto MapToResponseDto(Class classItem, int studentCount)
        {
            return new ClassResponseDto
            {
                ClassId = classItem.ClassId,
                ClassName = classItem.ClassName,
                Major = classItem.Major,
                SemesterId = classItem.SemesterId,
                SemesterName = classItem.Semester?.SemesterName,
                AcademicYear = classItem.Semester?.AcademicYear,
                IsActive = classItem.IsActive,
                StudentCount = studentCount
            };
        }

        // ✅ SỬA - MapToClassWithStudentsDto với đầy đủ properties
        private ClassWithStudentsDto MapToClassWithStudentsDto(Class classItem)
        {
            return new ClassWithStudentsDto
            {
                ClassId = classItem.ClassId,
                ClassName = classItem.ClassName,
                Major = classItem.Major,
                SemesterId = classItem.SemesterId,
                SemesterName = classItem.Semester?.SemesterName, // ✅ Đã có trong DTO
                AcademicYear = classItem.Semester?.AcademicYear, // ✅ Đã có trong DTO
                IsActive = classItem.IsActive,
                Students = classItem.Students.Select(MapStudentToResponseDto),
                StudentCount = classItem.Students.Count
            };
        }

        // ✅ SỬA - MapToClass với SemesterId
        private Class MapToClass(CreateClassDto dto)
        {
            return new Class
            {
                ClassId = dto.ClassId,
                ClassName = dto.ClassName,
                Major = dto.Major,
                SemesterId = dto.SemesterId,
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

        #endregion
    }
}
