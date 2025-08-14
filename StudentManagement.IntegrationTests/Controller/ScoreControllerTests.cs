using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentManagementSystem.Controllers;
using StudentManagementSystem.DTOs.Score;
using StudentManagementSystem.Services.Interfaces;
using Xunit;

namespace StudentManagement.IntegrationTests.Controllers
{
    public class ScoreControllerTests
    {
        private readonly Mock<IScoreService> _mockService;
        private readonly ScoreController _controller;

        public ScoreControllerTests()
        {
            _mockService = new Mock<IScoreService>();
            _controller = new ScoreController(_mockService.Object);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_IfFound()
        {
            var dto = new ScoreDto { ScoreId = 1, EnrollmentId = 100 };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_IfNotExist()
        {
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((ScoreDto)null!);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetByEnrollmentId_ShouldReturnOk()
        {
            var dto = new ScoreDto { ScoreId = 2, EnrollmentId = 200 };
            _mockService.Setup(s => s.GetByEnrollmentIdAsync(200)).ReturnsAsync(dto);

            var result = await _controller.GetByEnrollmentId(200);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_IfSuccess()
        {
            var dto = new ScoreCreateDto { EnrollmentId = 1, ProcessScore = 7, MidtermScore = 8, FinalScore = 9 };
            _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(true);

            var result = await _controller.Create(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_IfFail()
        {
            var dto = new ScoreCreateDto { EnrollmentId = 1 };
            _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(false);

            var result = await _controller.Create(dto);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_IfSuccess()
        {
            var dto = new ScoreUpdateDto { ScoreId = 1, EnrollmentId = 1, FinalScore = 10 };
            _mockService.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(true);

            var result = await _controller.Update(1, dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_IfIdMismatch()
        {
            var dto = new ScoreUpdateDto { ScoreId = 2, EnrollmentId = 1 };
            var result = await _controller.Update(1, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            bad.Value.Should().Be("ScoreId mismatch.");
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_IfFail()
        {
            var dto = new ScoreUpdateDto { ScoreId = 1, EnrollmentId = 1 };
            _mockService.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(false);

            var result = await _controller.Update(1, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_IfSuccess()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_IfFail()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(false);

            var result = await _controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Search_ShouldReturnOk()
        {
            var scores = new List<ScoreDto>
            {
                new ScoreDto { ScoreId = 1, EnrollmentId = 1 },
                new ScoreDto { ScoreId = 2, EnrollmentId = 2 }
            };
            _mockService.Setup(s => s.SearchScoresAsync("abc")).ReturnsAsync(scores);

            var result = await _controller.Search("abc");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().BeEquivalentTo(scores);
        }


    }
}
