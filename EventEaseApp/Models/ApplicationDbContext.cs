using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventEaseApp.Models // Change 'YourNamespace' to match your project
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) 
        {
        
        }

        public DbSet<Venue> Venue { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Event> Event { get; set; }
       
        public DbSet<Booking> Booking { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure unique bookings per event per venue
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.EventId, b.VenueId })
                .IsUnique();

            modelBuilder.Entity<EventType>().HasData(
      new EventType { EventTypeId = 1, Name = "Conference" },
      new EventType { EventTypeId = 2, Name = "Workshop" },
      new EventType { EventTypeId = 3, Name = "Meetup" },
      new EventType { EventTypeId = 4, Name = "Festival" },
      new EventType { EventTypeId = 5, Name = "Concert" },
      new EventType { EventTypeId = 6, Name = "Fundraiser" }
  );
        }
    }
}