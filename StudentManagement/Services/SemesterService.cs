using Microsoft.Extensions.Logging;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Semester;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services.Interfaces;

namespace StudentManagementSystem.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly ISemesterRepository _semesterRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SemesterService> _logger;

        public SemesterService(
            ISemesterRepository semesterRepository,
            ICacheService cacheService,
            ILogger<SemesterService> logger)
        {
            _semesterRepository = semesterRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<SemesterDto>> GetAllAsync()
        {
            const string cacheKey = "all_semesters";
            var cachedSemesters = await _cacheService.GetDataAsync<IEnumerable<SemesterDto>>(cacheKey);

            if (cachedSemesters != null)
            {
                _logger.LogInformation("Returning {Count} semesters from cache.", cachedSemesters.Count());
                return cachedSemesters;
            }

            var semesters = await _semesterRepository.GetAllAsync();
            var semesterDtos = semesters.Select(MapToDto).ToList();

            await _cacheService.SetDataAsync(cacheKey, semesterDtos, DateTimeOffset.Now.AddMinutes(30));
            _logger.LogInformation("Returning {Count} semesters from database.", semesterDtos.Count);

            return semesterDtos;
        }

        public async Task<IEnumerable<SemesterDto>> GetActiveSemstersAsync()
        {
            const string cacheKey = "active_semesters";
            var cachedSemesters = await _cacheService.GetDataAsync<IEnumerable<SemesterDto>>(cacheKey);

            if (cachedSemesters != null)
            {
                return cachedSemesters;
            }

            var semesters = await _semesterRepository.GetActiveSemstersAsync();
            var semesterDtos = semesters.Select(MapToDto).ToList();

            await _cacheService.SetDataAsync(cacheKey, semesterDtos, DateTimeOffset.Now.AddMinutes(30));
            return semesterDtos;
        }

        public async Task<SemesterDto?> GetByIdAsync(int id)
        {
            var cacheKey = $"semester_{id}";
            var cachedSemester = await _cacheService.GetDataAsync<SemesterDto>(cacheKey);

            if (cachedSemester != null)
            {
                return cachedSemester;
            }

            var semester = await _semesterRepository.GetByIdAsync(id);
            if (semester == null) return null;

            var semesterDto = MapToDto(semester);
            await _cacheService.SetDataAsync(cacheKey, semesterDto, DateTimeOffset.Now.AddMinutes(30));

            return semesterDto;
        }

        public async Task<SemesterDto?> GetSemesterWithClassesAsync(int semesterId)
        {
            var semester = await _semesterRepository.GetSemesterWithClassesAsync(semesterId);
            return semester == null ? null : MapToDto(semester);
        }

        public async Task<SemesterDto?> GetSemesterWithTeacherCoursesAsync(int semesterId)
        {
            var semester = await _semesterRepository.GetSemesterWithTeacherCoursesAsync(semesterId);
            return semester == null ? null : MapToDto(semester);
        }

        public async Task<SemesterDto?> GetSemesterWithEnrollmentsAsync(int semesterId)
        {
            var semester = await _semesterRepository.GetSemesterWithEnrollmentsAsync(semesterId);
            return semester == null ? null : MapToDto(semester);
        }

        public async Task<SemesterDto> CreateAsync(CreateSemesterDto createDto)
        {
            var semester = MapToSemester(createDto);
            var createdSemester = await _semesterRepository.AddAsync(semester);

            // Clear cache
            await _cacheService.RemoveByPatternAsync("semester");

            _logger.LogInformation("Created new semester with ID {SemesterId}.", createdSemester.SemesterId);
            return MapToDto(createdSemester);
        }

        public async Task<SemesterDto?> UpdateAsync(int id, UpdateSemesterDto updateDto)
        {
            var semester = await _semesterRepository.GetByIdAsync(id);
            if (semester == null) return null;

            semester.SemesterName = updateDto.SemesterName;
            semester.AcademicYear = updateDto.AcademicYear;
            semester.StartDate = updateDto.StartDate;
            semester.EndDate = updateDto.EndDate;
            semester.IsActive = updateDto.IsActive;


            await _semesterRepository.UpdateAsync(semester);

            // Clear cache
            await _cacheService.RemoveDataAsync($"semester_{id}");
            await _cacheService.RemoveByPatternAsync("semester");

            _logger.LogInformation("Updated semester with ID {SemesterId}.", id);

            return MapToDto(semester);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var canDelete = await _semesterRepository.CanDeleteSemesterAsync(id);
            if (!canDelete)
            {
                _logger.LogWarning("Cannot delete semester {SemesterId} because it has dependencies.", id);
                return false;
            }


            var semester = await _semesterRepository.GetByIdAsync(id);
            if (semester == null) return false;

            await _semesterRepository.DeleteAsync(semester);
            await _cacheService.RemoveDataAsync($"semester_{id}");
            await _cacheService.RemoveByPatternAsync("semester");

            _logger.LogInformation("Deleted semester with ID {SemesterId}.", id);
            return true; // Manually return true
        }

        public async Task<IEnumerable<SemesterDto>> GetSemestersByAcademicYearAsync(string academicYear)
        {
            var semesters = await _semesterRepository.GetSemestersByAcademicYearAsync(academicYear);
            return semesters.Select(MapToDto);
        }

        public async Task<(IEnumerable<SemesterDto> Semesters, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var (semesters, totalCount) = await _semesterRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, isActive);
            return (semesters.Select(MapToDto), totalCount);
        }

        public async Task<bool> IsSemesterNameExistsAsync(string semesterName, string academicYear, int? excludeSemesterId = null)
        {
            return await _semesterRepository.IsSemesterNameExistsAsync(semesterName, academicYear, excludeSemesterId);
        }

        // Helper methods
        private static SemesterDto MapToDto(Semester semester)
        {
            return new SemesterDto
            {
                SemesterId = semester.SemesterId,
                SemesterName = semester.SemesterName,
                AcademicYear = semester.AcademicYear,
                StartDate = semester.StartDate,
                EndDate = semester.EndDate,
                IsActive = semester.IsActive
            };
        }

        private static Semester MapToSemester(CreateSemesterDto dto)
        {
            return new Semester
            {
                SemesterName = dto.SemesterName,
                AcademicYear = dto.AcademicYear,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive
            };
        }
    }
}
