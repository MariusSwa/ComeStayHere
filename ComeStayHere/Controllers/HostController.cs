using ComeStayHere.Data;
using ComeStayHere.Models;
using ComeStayHere.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComeStayHere.Controllers
{
    [Authorize(Roles = "Host")]
    public class HostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HostController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var properties = await _context.HostProperties
                .Include(p => p.PropertyImages)
                .Where(p => p.HostId == user.Id)
                .ToListAsync();

            return View(properties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePropertyStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var property = await _context.HostProperties
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.HostId == user.Id);

            if (property == null)
            {
                return NotFound();
            }

            property.IsActive = !property.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Bookings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.Guest)
                .Where(b => b.Property != null &&
                            b.Property.HostId == user.Id)
                .OrderByDescending(b => b.CheckInDate)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(int id)
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
                    b.Property != null &&
                    b.Property.HostId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != BookingStatus.Pending)
            {
                return RedirectToAction(nameof(Bookings));
            }

            booking.Status = BookingStatus.Approved;

            var activityLog = new ActivityLog
            {
                UserId = user.Id,
                ActivityType = ActivityType.BookingStatusUpdate,
                Description = $"Booking {booking.Id} was approved.",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);

            var notification = new Notification
            {
                UserId = booking.GuestId,
                Message = "Your booking has been approved.",
                Type = NotificationType.BookingApproved,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Bookings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int id)
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
                    b.Property != null &&
                    b.Property.HostId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != BookingStatus.Pending)
            {
                return RedirectToAction(nameof(Bookings));
            }

            booking.Status = BookingStatus.Rejected;

            var activityLog = new ActivityLog
            {
                UserId = user.Id,
                ActivityType = ActivityType.BookingStatusUpdate,
                Description = $"Booking {booking.Id} was rejected.",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Bookings));
        }

        [HttpGet]
        public async Task<IActionResult> CheckInProcess(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var property = await _context.HostProperties
                .Include(p => p.CheckInProcess)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.HostId == user.Id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCheckInProcess(
    int propertyId,
    string title,
    string description,
    bool requiresDocument,
    string requiredDocumentType)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var property = await _context.HostProperties
                .Include(p => p.CheckInProcess)
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId &&
                    p.HostId == user.Id);

            if (property == null)
            {
                return NotFound();
            }

            if (property.CheckInProcess == null)
            {
                var checkInProcess = new CheckInProcess
                {
                    PropertyId = property.Id,
                    Title = title,
                    Description = description,
                    RequiresDocument = requiresDocument,
                    RequiredDocumentType =
                        requiresDocument ? requiredDocumentType : string.Empty
                };

                _context.CheckInProcesses.Add(checkInProcess);
            }
            else
            {
                property.CheckInProcess.Title = title;
                property.CheckInProcess.Description = description;
                property.CheckInProcess.RequiresDocument = requiresDocument;
                property.CheckInProcess.RequiredDocumentType =
                    requiresDocument ? requiredDocumentType : string.Empty;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(CheckInProcess),
                new { id = propertyId });
        }

        public async Task<IActionResult> GuestDocuments()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var documents = await _context.GuestDocuments
                .Include(d => d.GuestCheckIn)
                    .ThenInclude(c => c!.Booking)
                        .ThenInclude(b => b!.Property)
                .Where(d =>
                    d.GuestCheckIn != null &&
                    d.GuestCheckIn.Booking != null &&
                    d.GuestCheckIn.Booking.Property != null &&
                    d.GuestCheckIn.Booking.Property.HostId == user.Id)
                .ToListAsync();

            return View(documents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDocument(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var document = await _context.GuestDocuments
                .Include(d => d.GuestCheckIn)
                    .ThenInclude(c => c!.Booking)
                        .ThenInclude(b => b!.Property)
                .FirstOrDefaultAsync(d =>
                    d.Id == id &&
                    d.GuestCheckIn != null &&
                    d.GuestCheckIn.Booking != null &&
                    d.GuestCheckIn.Booking.Property != null &&
                    d.GuestCheckIn.Booking.Property.HostId == user.Id);

            if (document == null)
            {
                return NotFound();
            }

            document.Status = DocumentStatus.Verified;

            var guestCheckIn = document.GuestCheckIn;
            var booking = guestCheckIn!.Booking;

            guestCheckIn.Status = CheckInStatus.CheckedIn;
            booking!.Status = BookingStatus.CheckedIn;

            var activityLog = new ActivityLog
            {
                UserId = user.Id,
                ActivityType = ActivityType.BookingStatusUpdate,
                Description = $"Booking {booking.Id} was checked in.",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GuestDocuments));
        }

        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var property = await _context.HostProperties
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.HostId == user.Id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(HostProperty model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var property = await _context.HostProperties
                .FirstOrDefaultAsync(p =>
                    p.Id == model.Id &&
                    p.HostId == user.Id);

            if (property == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.Title) ||
                string.IsNullOrWhiteSpace(model.Description) ||
                string.IsNullOrWhiteSpace(model.Address) ||
                string.IsNullOrWhiteSpace(model.City) ||
                string.IsNullOrWhiteSpace(model.PropertyType) ||
                model.PricePerNight <= 0 ||
                model.CleaningFee < 0 ||
                model.ServiceFee < 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please enter valid property details.");

                return View(model);
            }

            property.Title = model.Title;
            property.Description = model.Description;
            property.PricePerNight = model.PricePerNight;
            property.CleaningFee = model.CleaningFee;
            property.ServiceFee = model.ServiceFee;
            property.Address = model.Address;
            property.City = model.City;
            property.PropertyType = model.PropertyType;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}