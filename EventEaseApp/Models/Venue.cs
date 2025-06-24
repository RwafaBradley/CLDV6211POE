using static EventEaseApp.Models.Booking;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApp.Models
{
    public class Venue
    {
        // [Key]
        [Key]
        public int VenueId { get; set; }

        [Required]
        public string VenueName { get; set; }
        [Required]
        public string Locations { get; set; }
        public int Capacity { get; set; }

        public string? ImageUrl { get; set; }



        //[Required]
        //[Range(0, 1, ErrorMessage = "IsAvailable must be 0 or 1")]

        [NotMapped]
        [MaxLength(20)]
        public string IsAvailable { get; set; } = "Unavailable";

[NotMapped]
        public IFormFile? ImageFile { get; set; }
        public ICollection<Booking> Booking { get; set; } = new List<Booking>();
    }
}
