using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;
using EMS.Web.Controllers;
using EMS.Web.Models.ViewModels;
using System.Threading.Tasks;
using FluentAssertions;

namespace EMS.Tests.Controllers
{
    public class AccountControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _controller = new AccountController(_context);
            
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var adminUser = new UserInfo
            {
                EmailId = "admin@test.com",
                UserName = "Test Admin",
                Role = "Admin",
                Password = "admin123"
            };
            
            var participantUser = new UserInfo
            {
                EmailId = "user@test.com",
                UserName = "Test User",
                Role = "Participant",
                Password = "user123"
            };
            
            _context.Users.AddRange(adminUser, participantUser);
            _context.SaveChanges();
        }

        [Fact]
        public void Login_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Login();
            
            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_ValidAdminCredentials_RedirectsToAdminDashboard()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "admin@test.com",
                Password = "admin123"
            };
            
            // Act
            var result = await _controller.Login(model);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("Admin", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_ValidParticipantCredentials_RedirectsToParticipantDashboard()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "user@test.com",
                Password = "user123"
            };
            
            // Act
            var result = await _controller.Login(model);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("Participant", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "wrong@test.com",
                Password = "wrong"
            };
            
            // Act
            var result = await _controller.Login(model);
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void Register_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Register();
            
            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Register_ValidNewUser_ReturnsRedirectToLogin()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "newuser@test.com",
                UserName = "New User",
                Password = "password123",
                ConfirmPassword = "password123"
            };
            
            // Act
            var result = await _controller.Register(model);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal(3, _context.Users.Count());
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "admin@test.com",
                UserName = "Duplicate User",
                Password = "password123",
                ConfirmPassword = "password123"
            };
            
            // Act
            var result = await _controller.Register(model);
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Email already registered", 
                _controller.ModelState["Email"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Register_PasswordMismatch_ReturnsViewWithError()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "mismatch@test.com",
                UserName = "Mismatch User",
                Password = "password123",
                ConfirmPassword = "different"
            };
            
            // Act
            var result = await _controller.Register(model);
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        public void Dispose()
        {
            _context.Dispose();
            _controller.Dispose();
        }
    }
}