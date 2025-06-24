using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApp.Models
{
    [Table("EventType")]
    public class EventType
    {

        [Key]
        public int EventTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Optional back-reference
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
