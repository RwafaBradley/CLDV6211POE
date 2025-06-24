using System;
using System.Data;
using System.IO;
using System.Reflection.Metadata;
using Azure.Storage.Blobs;
using EventEaseApp.Models;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EventEaseApp.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
        }
        public void PopulateAvailability(string selected)
        {
            var items = new List<SelectListItem>
    {
        // (optional) “All” choice
        new SelectListItem { Value = "", Text = "All" },

       new SelectListItem { Value = "Unavailable", Text = "Unavailable" },
 new SelectListItem { Value = "Available",   Text = "Available" },
    };

            // Pass the selected value in as a string
            ViewBag.AvailabilityList = new SelectList(
                items,               // source
                nameof(SelectListItem.Value),
                nameof(SelectListItem.Text),
                selected?.ToString() // pre-select
            );
        }
        public async Task<IActionResult> Index(string availability)
        {
            // 1) Load all venues with their bookings
            var venues = await _context.Venue
                               .Include(v => v.Booking)
                               .Include(v => v.Booking)
                                 .ThenInclude(b => b.Event)
                               .ToListAsync();

            foreach (var v in venues) //if an event has been created on any venue this wwill trigger till there are no events on it
            {
                bool busy = v.Booking.Any(b => b.Event != null);
                v.IsAvailable = busy ? "Unavailable" : "Available";
            }

            // 3) Filter in memory by the same availability parameter
            if (!string.IsNullOrEmpty(availability))
            {
                venues = venues
                           .Where(v => v.IsAvailable == availability)
                           .ToList();
            }

            // 4) Build the dropdown
            ViewBag.AvailabilityList = new SelectList(new[]
            {
        new { Value = "",            Text = "All" },
        new { Value = "Unavailable", Text = "Unavailable" },
        new { Value = "Available",   Text = "Available"   },
    }, "Value", "Text", availability);
            PopulateAvailability(availability);
            return View(venues);
        }
        public IActionResult Create()
        {

            var vm = new Venue { IsAvailable = "Unavailable" };
            PopulateAvailability(vm.IsAvailable);
               return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {


            if (ModelState.IsValid) {
                
                    if (venue.ImageFile != null)
                    {
                    PopulateAvailability(venue.IsAvailable);
                    var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);
                    
                    venue.ImageUrl = blobUrl;
                    }

                    else
                    {

                    }
                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue created succesfully.";
                return RedirectToAction(nameof(Index));



            }

            PopulateAvailability(venue.IsAvailable);
            return View(venue);
        }
    


        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();
            PopulateAvailability(venue.IsAvailable);
            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueId)
            
                return NotFound();
            

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                        PopulateAvailability(venue.IsAvailable);
                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);
                        
                        venue.ImageUrl = blobUrl;
                    }
                    else
                    {
                        //keep original image
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["ErrorMessage"] = "Venue updated succesfully.";

                }
                catch (DBConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                        return NotFound();
                    else
                        throw;

                }
                return RedirectToAction(nameof(Index));
            }
            PopulateAvailability(venue.IsAvailable);
            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            var hasBookings = await _context.Booking.AnyAsync(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "You are not allowed delete a venue with existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue deleted succesfully.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null) return NotFound();

            return View(venue);
        }



       private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
{
           var connectionString = "";
    var containerName = "";

    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(imageFile.FileName));

    var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
    {
        ContentType = imageFile.ContentType
    };

    using (var stream = imageFile.OpenReadStream())
    {
        await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });
    }
            return blobClient.Uri.ToString();
}



    }
}