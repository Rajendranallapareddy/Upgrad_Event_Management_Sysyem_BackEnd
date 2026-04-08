using Xunit;
using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;
using EMS.DAL.Data;
using EMS.DAL.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace EMS.Tests.Repository
{
    public class EventRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EventRepository _repository;

        public EventRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _repository = new EventRepository(_context);
            
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var events = new[]
            {
                new EventDetails
                {
                    EventId = Guid.NewGuid(),
                    EventName = "Active Future Event",
                    EventCategory = "Tech",
                    EventDate = DateTime.Now.AddDays(30),
                    Status = "Active"
                },
                new EventDetails
                {
                    EventId = Guid.NewGuid(),
                    EventName = "Inactive Event",
                    EventCategory = "Industrial",
                    EventDate = DateTime.Now.AddDays(20),
                    Status = "In-Active"
                },
                new EventDetails
                {
                    EventId = Guid.NewGuid(),
                    EventName = "Past Active Event",
                    EventCategory = "Tech",
                    EventDate = DateTime.Now.AddDays(-10),
                    Status = "Active"
                }
            };
            
            _context.Events.AddRange(events);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetActiveEventsAsync_ReturnsOnlyActiveFutureEvents()
        {
            // Act
            var result = await _repository.GetActiveEventsAsync();
            
            // Assert
            var events = result.ToList();
            events.Should().NotBeEmpty();
            events.All(e => e.Status == "Active").Should().BeTrue();
            events.All(e => e.EventDate >= DateTime.Now).Should().BeTrue();
        }

        [Fact]
        public async Task GetEventsByCategoryAsync_ReturnsEventsForSpecificCategory()
        {
            // Act
            var result = await _repository.GetEventsByCategoryAsync("Tech");
            
            // Assert
            var events = result.ToList();
            events.All(e => e.EventCategory == "Tech").Should().BeTrue();
        }

        [Fact]
        public async Task ToggleEventStatusAsync_TogglesStatus()
        {
            // Arrange
            var eventToToggle = _context.Events.First(e => e.Status == "Active");
            var originalStatus = eventToToggle.Status;
            
            // Act
            var result = await _repository.ToggleEventStatusAsync(eventToToggle.EventId);
            
            // Assert
            result.Should().BeTrue();
            var updatedEvent = await _context.Events.FindAsync(eventToToggle.EventId);
            updatedEvent.Status.Should().NotBe(originalStatus);
        }

        [Fact]
        public async Task AddAsync_AddsNewEvent()
        {
            // Arrange
            var newEvent = new EventDetails
            {
                EventId = Guid.NewGuid(),
                EventName = "Brand New Event",
                EventCategory = "Workshop",
                EventDate = DateTime.Now.AddDays(50),
                Status = "Active"
            };
            
            // Act
            var result = await _repository.AddAsync(newEvent);
            
            // Assert
            result.Should().NotBeNull();
            result.EventName.Should().Be("Brand New Event");
            var count = await _repository.GetAllAsync();
            count.Count().Should().Be(4);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectEvent()
        {
            // Arrange
            var expectedEvent = _context.Events.First();
            
            // Act
            var result = await _repository.GetByIdAsync(expectedEvent.EventId);
            
            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().Be(expectedEvent.EventId);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEvent()
        {
            // Arrange
            var eventToUpdate = _context.Events.First();
            eventToUpdate.EventName = "Updated Event Name";
            
            // Act
            var result = await _repository.UpdateAsync(eventToUpdate);
            
            // Assert
            result.EventName.Should().Be("Updated Event Name");
            var updatedEvent = await _context.Events.FindAsync(eventToUpdate.EventId);
            updatedEvent.EventName.Should().Be("Updated Event Name");
        }

        [Fact]
        public async Task DeleteAsync_RemovesEvent()
        {
            // Arrange
            var eventToDelete = _context.Events.First();
            var initialCount = _context.Events.Count();
            
            // Act
            await _repository.DeleteAsync(eventToDelete.EventId);
            
            // Assert
            var remainingEvents = await _repository.GetAllAsync();
            remainingEvents.Count().Should().Be(initialCount - 1);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrueForExistingEvent()
        {
            // Arrange
            var existingEvent = _context.Events.First();
            
            // Act
            var result = await _repository.ExistsAsync(e => e.EventId == existingEvent.EventId);
            
            // Assert
            result.Should().BeTrue();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}