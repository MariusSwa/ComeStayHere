using ComeStayHere.Data;
using ComeStayHere.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ComeStayHere.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

        public async Task<IActionResult> HostApplications()
        {
            var applications = await _context.HostApplications
                .Include(a => a.User)
                .Include(a => a.Property)
                    .ThenInclude(p => p!.PropertyImages)
                .Where(a => a.Status == ApplicationStatus.Pending)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveHostApplication(int id)
        {
            var application = await _context.HostApplications
                .Include(a => a.User)
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null || application.User == null)
            {
                return NotFound();
            }

            if (application.Status != ApplicationStatus.Pending)
            {
                return RedirectToAction(nameof(HostApplications));
            }

            var addRoleResult = await _userManager.AddToRoleAsync(
                application.User,
                "Host");

            if (!addRoleResult.Succeeded)
            {
                return RedirectToAction(nameof(HostApplications));
            }

            if (await _userManager.IsInRoleAsync(application.User, "Guest"))
            {
                var removeRoleResult = await _userManager.RemoveFromRoleAsync(
                    application.User,
                    "Guest");

                if (!removeRoleResult.Succeeded)
                {
                    return RedirectToAction(nameof(HostApplications));
                }
            }

            application.Status = ApplicationStatus.Approved;
            application.ReviewedAt = DateTime.Now;
            application.ReviewedBy = _userManager.GetUserId(User);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HostApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectHostApplication(int id)
        {
            var application = await _context.HostApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            if (application.Status != ApplicationStatus.Pending)
            {
                return RedirectToAction(nameof(HostApplications));
            }

            application.Status = ApplicationStatus.Rejected;
            application.ReviewedAt = DateTime.Now;
            application.ReviewedBy = _userManager.GetUserId(User);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HostApplications));
        }

        public async Task<IActionResult> Index()
        {
            var pendingApplications = await _context.HostApplications
                .CountAsync(a => a.Status == ApplicationStatus.Pending);

            var activeProperties = await _context.HostProperties
                .CountAsync(p => p.IsActive);

            var pendingBookings = await _context.Bookings
                .CountAsync(b => b.Status == BookingStatus.Pending);

            ViewBag.PendingApplications = pendingApplications;
            ViewBag.ActiveProperties = activeProperties;
            ViewBag.PendingBookings = pendingBookings;

            return View();
        }
    }
}