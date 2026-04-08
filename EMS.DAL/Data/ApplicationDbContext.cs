using Microsoft.EntityFrameworkCore;
using EMS.DAL.Models;

namespace EMS.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserInfo> Users { get; set; }
        public DbSet<EventDetails> Events { get; set; }
        public DbSet<SpeakersDetails> Speakers { get; set; }
        public DbSet<SessionInfo> Sessions { get; set; }
        public DbSet<ParticipantEventDetails> ParticipantEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SessionInfo>()
                .HasOne(s => s.Event)
                .WithMany(e => e.Sessions)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionInfo>()
                .HasOne(s => s.Speaker)
                .WithMany(sp => sp.Sessions)
                .HasForeignKey(s => s.SpeakerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ParticipantEventDetails>()
                .HasOne(p => p.Participant)
                .WithMany(u => u.ParticipantEvents)
                .HasForeignKey(p => p.ParticipantEmailId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ParticipantEventDetails>()
                .HasOne(p => p.Event)
                .WithMany(e => e.ParticipantEvents)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Admin User
            modelBuilder.Entity<UserInfo>().HasData(new UserInfo
            {
                EmailId = "admin@upgrad.com",
                UserName = "Administrator",
                Role = "Admin",
                Password = "admin123"
            });
        }
    }
}