using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagementSystem.DTOs.Analytics;

namespace StudentManagementSystem.Data.Interfaces
{
    public interface IStudentAnalyticsRepository
    {
        Task<(IEnumerable<StudentAvgDto> Items, int TotalCount)>
            GetTopStudentsAsync(int page, int size, int? semesterId = null);
    }
}
