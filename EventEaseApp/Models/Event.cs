using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // if you use [ForeignKey]
using EventEaseApp.Models;

namespace EventEaseApp.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }
        public string Descriptions { get; set; }

        public int? VenueId { get; set; }  // Venue is optional at creation
        public Venue? Venue { get; set; }  // Navigation property

        public int? EventTypeId { get; set; }

        // **new**: navigation property
        public EventType? EventType { get; set; } = null!;
    }
}
