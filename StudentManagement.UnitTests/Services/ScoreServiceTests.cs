using Xunit;
using FluentAssertions;
using StudentManagementSystem.Models;
using System;

namespace StudentManagementSystem.Tests.Models
{
    public class ScoreModelTests
    {
        // -----------------------------------------------------------------------------
        #region CalculateTotalScore Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public void CalculateTotalScore_KhiCoDayDuDiem_TraVeTongDiemChinhXac()
        {
            // Arrange
            // Trọng số: Process (20%), Midterm (30%), Final (50%)
            var score = new Score
            {
                ProcessScore = 8.0m,    // 8.0 * 0.2 = 1.6
                MidtermScore = 7.0m,    // 7.0 * 0.3 = 2.1
                FinalScore = 9.0m       // 9.0 * 0.5 = 4.5
            };
            // Tổng điểm mong đợi: 1.6 + 2.1 + 4.5 = 8.2
            var expectedTotalScore = 8.2m;

            // Act
            var result = score.CalculateTotalScore();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedTotalScore);
        }

        [Fact]
        public void CalculateTotalScore_KhiKetQuaCanLamTron_TraVeGiaTriDaLamTronDen2ChuSoThapPhan()
        {
            // Arrange
            var score = new Score
            {
                ProcessScore = 7.7m,   // 7.7 * 0.2 = 1.54
                MidtermScore = 8.3m,   // 8.3 * 0.3 = 2.49
                FinalScore = 9.4m    // 9.4 * 0.5 = 4.70
            };
            // Tổng điểm chưa làm tròn: 1.54 + 2.49 + 4.70 = 8.73
            var expectedTotalScore = 8.73m;

            // Act
            var result = score.CalculateTotalScore();

            // Assert
            result.Should().Be(expectedTotalScore);
        }

        public static IEnumerable<object[]> InvalidScoreData => new List<object[]>
        {
            new object[] { (decimal?)null, 7.0m, 8.0m },
            new object[] { 10.0m, (decimal?)null, 8.0m },
            new object[] { 10.0m, 7.0m, (decimal?)null }
        };

        [Theory]
        [MemberData(nameof(InvalidScoreData))]
        public void CalculateTotalScore_KhiThieuMotTrongCacDauDiem_TraVeNull(decimal? process, decimal? midterm, decimal? final)
        {
            var score = new Score
            {
                ProcessScore = process,
                MidtermScore = midterm,
                FinalScore = final
            };

            var result = score.CalculateTotalScore();

            result.Should().BeNull();
        }


        #endregion

        // -----------------------------------------------------------------------------
        #region CalculateIsPassed Tests
        // -----------------------------------------------------------------------------

        [Fact]
        public void CalculateIsPassed_KhiTongDiemLonHonHoacBangNguong_TraVeTrue()
        {
            // Arrange
            // Tổng điểm: 5*0.2 + 5*0.3 + 3*0.5 = 1 + 1.5 + 1.5 = 4.0
            var score = new Score { ProcessScore = 5.0m, MidtermScore = 5.0m, FinalScore = 3.0m };

            // Act
            var result = score.CalculateIsPassed();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeTrue();
        }

        [Fact]
        public void CalculateIsPassed_KhiTongDiemDuoiNguong_TraVeFalse()
        {
            // Arrange
            // Tổng điểm: 4*0.2 + 4*0.3 + 3*0.5 = 0.8 + 1.2 + 1.5 = 3.5
            var score = new Score { ProcessScore = 4.0m, MidtermScore = 4.0m, FinalScore = 3.0m };

            // Act
            var result = score.CalculateIsPassed();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeFalse();
        }

        [Fact]
        public void CalculateIsPassed_KhiDiemChuaDayDu_TraVeNull()
        {
            // Arrange
            var score = new Score { ProcessScore = 10.0m, MidtermScore = 10.0m, FinalScore = null };

            // Act
            var result = score.CalculateIsPassed();

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}
