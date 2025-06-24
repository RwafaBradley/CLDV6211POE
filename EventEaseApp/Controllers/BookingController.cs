using EventEaseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEaseApp.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
    string searchString,
    int? searchTypeId,
    int? venueId,
    DateTime? startDate,
    DateTime? endDate,
    string availability)
        {
            // 1) Optional: seed data for demo/testing
            if (!_context.EventTypes.Any())
            {
                _context.EventTypes.AddRange(
                    new EventType { Name = "Conference" },
                    new EventType { Name = "Workshop" },
                    new EventType { Name = "Meetup" },
                    new EventType { Name = "Concert" },
                    new EventType { Name = "Fundraiser" },
                    
                    new EventType { Name = "Seminar" },
                    new EventType { Name = "Party" }
                );
                await _context.SaveChangesAsync();
            }

            if (!_context.Venue.Any())
            {
                _context.Venue.AddRange(
                    new Venue { VenueName = "Venue A", Locations = "Location A", Capacity = 100 },
                    new Venue { VenueName = "Venue B", Locations = "Location B", Capacity = 200 }
                );
                await _context.SaveChangesAsync();
            }

            // 2) Load all bookings with their Event and Venue
            var list = await _context.Booking
                .Include(b => b.Event).ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .ToListAsync();

            // 3) Compute IsAvailable for each venue based on whether it has any event
            var venueHasEvent = _context.Event
                .GroupBy(e => e.VenueId)
                .ToDictionary(g => g.Key, g => true);

            foreach (var booking in list)
            {
                if (booking.VenueId.HasValue && venueHasEvent.ContainsKey(booking.VenueId.Value))
                {
                    booking.Venue.IsAvailable = "Unavailable";
                }
                else
                {
                    booking.Venue.IsAvailable = "Available";
                }
            }

            // 4) Apply in-memory filters
            if (!string.IsNullOrEmpty(searchString))
            {
                bool isNum = int.TryParse(searchString, out int bid);
                list = list.Where(b =>
                    (isNum && b.BookingId == bid) ||
                    b.Event.EventName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.Venue.VenueName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (searchTypeId.HasValue)
                list = list.Where(b => b.Event.EventTypeId == searchTypeId.Value).ToList();

            if (venueId.HasValue)
                list = list.Where(b => b.VenueId == venueId.Value).ToList();
            if (startDate.HasValue && endDate.HasValue)
            {
                var sd = startDate.Value.Date;
                var ed = endDate.Value.Date.AddDays(1);

                list = list.Where(b =>
                    b.BookingDate >= sd &&
                    b.BookingDate < ed
                ).ToList();
            }

            if (!string.IsNullOrEmpty(availability))
                list = list.Where(b => b.Venue.IsAvailable == availability).ToList();

            // 5) View data for dropdowns and filters
            ViewBag.SearchString = searchString;
            ViewBag.EventTypes = new SelectList(_context.EventTypes, "EventTypeId", "Name", searchTypeId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", venueId);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.AvailabilityList = new SelectList(new[]
            {
        new { Value = "",            Text = "All" },
        new { Value = "Unavailable", Text = "Unavailable" },
        new { Value = "Available",   Text = "Available"   },
    }, "Value", "Text", availability);

            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.VenueId = new SelectList(_context.Venue, "VenueId", "Locations");
            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName");
            return View(new Booking());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var selectedEvent = await _context.Event.FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            if (selectedEvent == null)
            {

                ModelState.AddModelError("", "Selected Event not found");
                ViewBag.VenueId = new SelectList(_context.Venue, "VenueId", "Locations", booking.VenueId);
                ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
                return View(booking);
            }

            var conflict = await _context.Booking
                          .Include(b => b.Event)
                          .AnyAsync(b => b.VenueId == booking.VenueId &&
                          b.Event.EventDate.Date == selectedEvent.EventDate.Date);

            if (conflict)
            {

                ModelState.AddModelError("", "This venue is already booked for that date");
            }

                if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.VenueId = new SelectList(_context.Venue, "VenueId", "Locations", booking.VenueId);
            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }    
            ViewBag.VenueId = new SelectList(_context.Venue, "VenueId", "Locations", booking.VenueId);
            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            return View(booking);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
          
            ViewBag.VenueId = new SelectList(_context.Venue, "VenueId", "Locations", booking.VenueId);
            ViewBag.EventId = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            return View(booking);
        }
    


public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}