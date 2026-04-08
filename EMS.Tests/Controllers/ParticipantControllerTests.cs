using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;
using EMS.Web.Controllers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FluentAssertions;

namespace EMS.Tests.Controllers
{
    public class ParticipantControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ParticipantController _controller;

        public ParticipantControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _controller = new ParticipantController(_context);
            
            SeedDatabase();
            SetupUserContext();
        }

        private void SeedDatabase()
        {
            var user = new UserInfo
            {
                EmailId = "participant@test.com",
                UserName = "Test Participant",
                Role = "Participant",
                Password = "pass123"
            };
            _context.Users.Add(user);
            
            var event1 = new EventDetails
            {
                EventId = Guid.NewGuid(),
                EventName = "Tech Summit 2024",
                EventCategory = "Tech",
                EventDate = DateTime.Now.AddDays(30),
                Status = "Active"
            };
            
            var event2 = new EventDetails
            {
                EventId = Guid.NewGuid(),
                EventName = "Industry Meetup",
                EventCategory = "Industrial",
                EventDate = DateTime.Now.AddDays(45),
                Status = "Active"
            };
            
            _context.Events.AddRange(event1, event2);
            
            var registration = new ParticipantEventDetails
            {
                Id = Guid.NewGuid(),
                ParticipantEmailId = "participant@test.com",
                EventId = event1.EventId,
                IsAttended = false
            };
            
            _context.ParticipantEvents.Add(registration);
            _context.SaveChanges();
        }

        private void SetupUserContext()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, "participant@test.com"),
                new Claim(ClaimTypes.Name, "Test Participant"),
                new Claim(ClaimTypes.Role, "Participant")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task Dashboard_ReturnsViewWithUserRegistrations()
        {
            // Act
            var result = await _controller.Dashboard();
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ParticipantEventDetails>>(viewResult.Model);
            model.Should().NotBeEmpty();
            model.Count.Should().Be(1);
        }

        [Fact]
        public async Task RegisterForEvent_NewEvent_AddsRegistration()
        {
            // Arrange
            var newEvent = await _context.Events.FirstAsync(e => e.EventName == "Industry Meetup");
            var initialCount = _context.ParticipantEvents.Count();
            
            // Act
            var result = await _controller.RegisterForEvent(newEvent.EventId);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal(initialCount + 1, _context.ParticipantEvents.Count());
        }

        [Fact]
        public async Task RegisterForEvent_AlreadyRegistered_DoesNotDuplicate()
        {
            // Arrange
            var existingEvent = await _context.Events.FirstAsync(e => e.EventName == "Tech Summit 2024");
            var initialCount = _context.ParticipantEvents.Count();
            
            // Act
            var result = await _controller.RegisterForEvent(existingEvent.EventId);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal(initialCount, _context.ParticipantEvents.Count());
        }

        [Fact]
        public async Task MyEvents_ReturnsViewWithRegisteredEvents()
        {
            // Act
            var result = await _controller.MyEvents();
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ParticipantEventDetails>>(viewResult.Model);
            model.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CancelRegistration_RemovesRegistration()
        {
            // Arrange
            var registration = await _context.ParticipantEvents.FirstAsync();
            var initialCount = _context.ParticipantEvents.Count();
            
            // Act
            var result = await _controller.CancelRegistration(registration.Id);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyEvents", redirectResult.ActionName);
            Assert.Equal(initialCount - 1, _context.ParticipantEvents.Count());
        }

        public void Dispose()
        {
            _context.Dispose();
            _controller.Dispose();
        }
    }
}