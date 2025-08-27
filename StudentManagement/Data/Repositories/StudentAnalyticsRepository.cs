using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentManagementSystem.Data.Interfaces;
using StudentManagementSystem.DTOs.Analytics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace StudentManagementSystem.Data.Repositories
{
    public class StudentAnalyticsRepository : IStudentAnalyticsRepository
    {
        private readonly string _connectionString;

        public StudentAnalyticsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection'.");
        }

        public async Task<(IEnumerable<StudentAvgDto> Items, int TotalCount)>
            GetTopStudentsAsync(int page, int size, int? semesterId = null)
        {
            const string sql = @"
            -- ========= DATA =========
            WITH Enr AS (
                SELECT e.EnrollmentId, e.StudentId
                FROM dbo.Enrollments e
                JOIN dbo.TeacherCourses tc ON e.TeacherCourseId = tc.TeacherCourseId
                WHERE (@SemesterId IS NULL OR tc.SemesterId = @SemesterId)
            ),
            ScoresCalc AS (
                SELECT  sc.EnrollmentId,
                        CAST(ROUND(
                            CASE WHEN sc.ProcessScore IS NOT NULL
                                AND sc.MidtermScore IS NOT NULL
                                AND sc.FinalScore   IS NOT NULL
                                THEN (sc.ProcessScore*0.2 + sc.MidtermScore*0.3 + sc.FinalScore*0.5)
                                ELSE NULL END, 2) AS DECIMAL(5,2)) AS TotalScoreCalc
                FROM dbo.Scores sc
            )
            SELECT  s.StudentId,
                    s.FullName,
                    c.ClassName,
                    CAST(AVG(sx.TotalScoreCalc) AS DECIMAL(5,2)) AS AverageScore
            FROM dbo.Students s
            JOIN dbo.Classes c   ON s.ClassId = c.ClassId
            JOIN Enr e           ON s.StudentId = e.StudentId
            JOIN ScoresCalc sx   ON e.EnrollmentId = sx.EnrollmentId
            GROUP BY s.StudentId, s.FullName, c.ClassName
            ORDER BY AverageScore DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

            -- ========= COUNT =========
            WITH Enr AS (
                SELECT e.EnrollmentId, e.StudentId
                FROM dbo.Enrollments e
                JOIN dbo.TeacherCourses tc ON e.TeacherCourseId = tc.TeacherCourseId
                WHERE (@SemesterId IS NULL OR tc.SemesterId = @SemesterId)
            ),
            ScoresCalc AS (
                SELECT  sc.EnrollmentId,
                        CAST(ROUND(
                            CASE WHEN sc.ProcessScore IS NOT NULL
                                AND sc.MidtermScore IS NOT NULL
                                AND sc.FinalScore   IS NOT NULL
                                THEN (sc.ProcessScore*0.2 + sc.MidtermScore*0.3 + sc.FinalScore*0.5)
                                ELSE NULL END, 2) AS DECIMAL(5,2)) AS TotalScoreCalc
                FROM dbo.Scores sc
            )
            SELECT COUNT(*) FROM (
                SELECT s.StudentId
                FROM dbo.Students s
                JOIN Enr e         ON s.StudentId = e.StudentId
                JOIN ScoresCalc sx ON e.EnrollmentId = sx.EnrollmentId
                GROUP BY s.StudentId
            ) t;
            ";

            var skip = (page - 1) * size;

            await using var conn = new SqlConnection(_connectionString);
            var multi = await conn.QueryMultipleAsync(sql, new
            {
                Skip = skip,
                Take = size,
                SemesterId = semesterId
            });

            var items = await multi.ReadAsync<StudentAvgDto>();
            var total = await multi.ReadFirstAsync<int>();
            return (items, total);
        }
    }
}
