using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using StudentManagementSystem;
using StudentManagementSystem.DTOs.Auth;
using Xunit;
using System.Text.Json.Serialization;

namespace StudentManagement.IntegrationTests.Controller
{
    public class AuthControllerTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_Should_Return_Token_When_Using_Seeded_Account()
        {
            // Arrange: tài khoản đã seed trong CustomWebAppFactory
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content); // In ra để debug nếu lỗi

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.False(string.IsNullOrEmpty(token!.AccessToken));
            Assert.False(string.IsNullOrEmpty(token.RefreshToken));
        }

        [Fact]
        public async Task Login_Should_Return_Unauthorized_If_Wrong_Credentials()
        {
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "sai_mat_khau"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
