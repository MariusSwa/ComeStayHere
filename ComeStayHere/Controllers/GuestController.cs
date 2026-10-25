using ComeStayHere.Data;
using ComeStayHere.Models;
using ComeStayHere.Models.Enums;
using ComeStayHere.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ComeStayHere.Controllers
{
    [Authorize(Roles = "Guest")]
    public class GuestController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public GuestController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
            {
                _context = context;
                _userManager = userManager;
                _webHostEnvironment = webHostEnvironment;
            }

        public async Task<IActionResult> Index(string? city, string? propertyType){
            var properties = _context.HostProperties
                .Include(p => p.PropertyImages)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
            {
                properties = properties
                    .Where(p => p.City.Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(propertyType))
            {
                properties = properties
                    .Where(p => p.PropertyType.Contains(propertyType));
            }

            ViewBag.City = city;
            ViewBag.PropertyType = propertyType;

            return View(await properties.ToListAsync());
        }

        public async Task<IActionResult> Details(int id){
            var property = await _context.HostProperties
                .Include(p => p.PropertyImages)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = model.PropertyId });
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var property = await _context.HostProperties
                .FirstOrDefaultAsync(p =>
                    p.Id == model.PropertyId &&
                    p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            if (model.CheckInDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Check-in date cannot be in the past.");

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.PropertyId });
            }

            if (model.CheckOutDate <= model.CheckInDate)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Check-out date must be after the check-in date.");

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.PropertyId });
            }

            var nights =
                (model.CheckOutDate - model.CheckInDate).Days;

            var totalPrice =
                (property.PricePerNight * nights)
                + property.CleaningFee
                + property.ServiceFee;

            var booking = new Booking
            {
                PropertyId = property.Id,
                GuestId = user.Id,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                NumberOfTravelers = model.NumberOfTravelers,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(BookingSuccess),
                new { id = booking.Id });
        }

        public async Task<IActionResult> BookingSuccess(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.GuestId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                    .ThenInclude(p => p!.PropertyImages)
                .Where(b => b.GuestId == user.Id)
                .OrderByDescending(b => b.CheckInDate)
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> CheckIn(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var booking = await _context.Bookings
                .Include(b => b.Property)
                    .ThenInclude(p => p!.CheckInProcess)
                .Include(b => b.GuestCheckIn)
                    .ThenInclude(c => c!.Documents)
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.GuestId == user.Id &&
                    b.Status == BookingStatus.Approved);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(
    int bookingId,
    string documentType,
    IFormFile document)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var booking = await _context.Bookings
                .Include(b => b.GuestCheckIn)
                .FirstOrDefaultAsync(b =>
                    b.Id == bookingId &&
                    b.GuestId == user.Id &&
                    b.Status == BookingStatus.Approved);

            if (booking == null)
            {
                return NotFound();
            }

            if (document == null || document.Length == 0)
            {
                return RedirectToAction(
                    nameof(CheckIn),
                    new { id = bookingId });
            }

            var guestCheckIn = booking.GuestCheckIn;

            if (guestCheckIn == null)
            {
                guestCheckIn = new GuestCheckIn
                {
                    BookingId = booking.Id,
                    CheckInDate = booking.CheckInDate,
                    Status = CheckInStatus.Pending
                };

                _context.GuestCheckIns.Add(guestCheckIn);
                await _context.SaveChangesAsync();
            }

            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "documents");

            Directory.CreateDirectory(uploadsFolder);

            var fileName =
                Guid.NewGuid().ToString() +
                Path.GetExtension(document.FileName);

            var filePath = Path.Combine(
                uploadsFolder,
                fileName);

            using (var fileStream =
                   new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(fileStream);
            }

            var guestDocument = new GuestDocument
            {
                GuestCheckInId = guestCheckIn.Id,
                DocumentType = documentType,
                FileUrl = "/uploads/documents/" + fileName,
                Status = DocumentStatus.Pending
            };

            _context.GuestDocuments.Add(guestDocument);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(CheckIn),
                new { id = bookingId });
        }
    }
}