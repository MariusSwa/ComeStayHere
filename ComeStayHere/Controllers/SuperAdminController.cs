using ComeStayHere.Data;
using ComeStayHere.Models;
using ComeStayHere.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ComeStayHere.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public SuperAdminController(
             UserManager<ApplicationUser> userManager,
             ApplicationDbContext context)
                {
                    _userManager = userManager;
                    _context = context;
                }

        public async Task<IActionResult> Index()
        {
            var guests = await _userManager.GetUsersInRoleAsync("Guest");

            return View(guests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Guest"))
            {
                return RedirectToAction(nameof(Index));
            }

            var addResult = await _userManager.AddToRoleAsync(user, "Admin");

            if (!addResult.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            var removeResult = await _userManager.RemoveFromRoleAsync(user, "Guest");

            var activityLog = new ActivityLog
            {
                UserId = user.Id,
                ActivityType = ActivityType.RoleChange,
                Description = "User role changed from Guest to Admin.",
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);

            await _context.SaveChangesAsync();

            if (!removeResult.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}