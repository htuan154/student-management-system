using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using System.Linq;
using BCrypt.Net;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Xoá cấu hình DbContext cũ (ApplicationDbContext)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Thêm ApplicationDbContext dùng InMemory
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Build provider và seed dữ liệu
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                // Thêm role "Admin"
                db.Roles.Add(new Role
                {
                    RoleId = "R002",
                    RoleName = "Admin",
                    Description = "Quản trị viên"
                });

                // Thêm tài khoản người dùng test
                db.Users.Add(new User
                {
                    UserId = "U001",
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    RoleId = "R002",
                    IsActive = true
                });

                db.SaveChanges();
            }
        });
    }
}
