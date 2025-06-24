using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApp.Models
{
   
        public class Booking
        {
            [Key]
            public int BookingId { get; set; }

            
            public int? EventId { get; set; }

            
            public int? VenueId { get; set; }

        
        [Display(Name = "Booking Date & Time")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime BookingDate { get; set; }

            [ForeignKey("EventId")]
            public Event? Event { get; set; }

            [ForeignKey("VenueId")]
            public Venue? Venue { get; set; }
        

    }
}
