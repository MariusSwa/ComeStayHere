using ComeStayHere.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ComeStayHere.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SuperAdminController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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

            var addResult =
                await _userManager.AddToRoleAsync(user, "Admin");

            if (!addResult.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            var removeResult =
                await _userManager.RemoveFromRoleAsync(user, "Guest");

            if (!removeResult.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}