using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;
using EMS.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace EMS.Tests.Controllers
{
    public class AdminControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _controller = new AdminController(_context);
            
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Seed test data
            var events = new List<EventDetails>
            {
                new EventDetails
                {
                    EventId = Guid.NewGuid(),
                    EventName = "Tech Conference 2024",
                    EventCategory = "Tech & Innovations",
                    EventDate = DateTime.Now.AddDays(30),
                    Status = "Active"
                },
                new EventDetails
                {
                    EventId = Guid.NewGuid(),
                    EventName = "Industrial Expo",
                    EventCategory = "Industrial Events",
                    EventDate = DateTime.Now.AddDays(45),
                    Status = "Active"
                }
            };
            
            _context.Events.AddRange(events);
            
            var speakers = new List<SpeakersDetails>
            {
                new SpeakersDetails { SpeakerId = Guid.NewGuid(), SpeakerName = "John Doe" },
                new SpeakersDetails { SpeakerId = Guid.NewGuid(), SpeakerName = "Jane Smith" }
            };
            
            _context.Speakers.AddRange(speakers);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Dashboard_ReturnsViewResult()
        {
            // Act
            var result = await _controller.Dashboard();
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public async Task Events_ReturnsViewWithEvents()
        {
            // Act
            var result = await _controller.Events();
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<EventDetails>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task CreateEvent_ValidEvent_ReturnsRedirectToEvents()
        {
            // Arrange
            var newEvent = new EventDetails
            {
                EventId = Guid.NewGuid(),
                EventName = "New Test Event",
                EventCategory = "Workshop",
                EventDate = DateTime.Now.AddDays(60),
                Status = "Active",
                Description = "Test description"
            };
            
            // Act
            var result = await _controller.CreateEvent(newEvent);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Events", redirectResult.ActionName);
            Assert.Equal(3, _context.Events.Count());
        }

        [Fact]
        public async Task CreateEvent_InvalidDate_ReturnsView()
        {
            // Arrange
            var newEvent = new EventDetails
            {
                EventId = Guid.NewGuid(),
                EventName = "Past Event",
                EventCategory = "Workshop",
                EventDate = DateTime.Now.AddDays(-10),
                Status = "Active"
            };
            
            // Act
            var result = await _controller.CreateEvent(newEvent);
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task EditEvent_ValidId_ReturnsViewWithEvent()
        {
            // Arrange
            var eventToEdit = _context.Events.First();
            
            // Act
            var result = await _controller.EditEvent(eventToEdit.EventId);
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EventDetails>(viewResult.Model);
            Assert.Equal(eventToEdit.EventId, model.EventId);
        }

        [Fact]
        public async Task EditEvent_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.EditEvent(Guid.NewGuid());
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_ValidId_RemovesEvent()
        {
            // Arrange
            var eventToDelete = _context.Events.First();
            var initialCount = _context.Events.Count();
            
            // Act
            var result = await _controller.DeleteEvent(eventToDelete.EventId);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Events", redirectResult.ActionName);
            Assert.Equal(initialCount - 1, _context.Events.Count());
        }

        [Fact]
        public async Task ToggleEventStatus_ValidId_TogglesStatus()
        {
            // Arrange
            var eventToToggle = _context.Events.First();
            var originalStatus = eventToToggle.Status;
            
            // Act
            var result = await _controller.ToggleEventStatus(eventToToggle.EventId);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            var updatedEvent = await _context.Events.FindAsync(eventToToggle.EventId);
            Assert.NotEqual(originalStatus, updatedEvent.Status);
        }

        [Fact]
        public async Task Speakers_ReturnsViewWithSpeakers()
        {
            // Act
            var result = await _controller.Speakers();
            
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<SpeakersDetails>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task CreateSpeaker_ValidSpeaker_AddsSpeaker()
        {
            // Arrange
            var newSpeaker = new SpeakersDetails
            {
                SpeakerId = Guid.NewGuid(),
                SpeakerName = "New Speaker"
            };
            
            // Act
            var result = await _controller.CreateSpeaker(newSpeaker);
            
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Speakers", redirectResult.ActionName);
            Assert.Equal(3, _context.Speakers.Count());
        }

        public void Dispose()
        {
            _context.Dispose();
            _controller.Dispose();
        }
    }
}