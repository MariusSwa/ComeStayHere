using ComeStayHere.Data;
using ComeStayHere.ViewModels;
using ComeStayHere.Models;
using ComeStayHere.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComeStayHere.Controllers
{
    [Authorize(Roles = "Guest")]
    public class HostApplicationController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HostApplicationController(
             ApplicationDbContext context,
             UserManager<ApplicationUser> userManager,
             IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Apply()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(HostApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var hasPendingApplication = await _context.HostApplications
            .AnyAsync(a => a.UserId == user.Id && a.Status == ApplicationStatus.Pending);

            if (hasPendingApplication)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You already have a pending Host application.");

                return View(model);
            }

            var property = new HostProperty
            {
                Title = model.Title,
                Description = model.Description,
                PricePerNight = model.PricePerNight,
                Address = model.Address,
                City = model.City,
                PropertyType = model.PropertyType,
                HostId = user.Id,
                IsActive = false
            };

            _context.HostProperties.Add(property);
            await _context.SaveChangesAsync();

            if (model.PropertyImage != null)
            {
                var uploadsFolder = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads",
                    "properties");

                Directory.CreateDirectory(uploadsFolder);

                var fileName =
                    Guid.NewGuid().ToString() +
                    Path.GetExtension(model.PropertyImage.FileName);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await model.PropertyImage.CopyToAsync(fileStream);
                }

                var propertyImage = new PropertyImage
                {
                    PropertyId = property.Id,
                    ImageUrl = "/uploads/properties/" + fileName
                };

                _context.PropertyImages.Add(propertyImage);
            }

            var application = new HostApplication
            {
                UserId = user.Id,
                PropertyId = property.Id,
                Status = ApplicationStatus.Pending,
                ApplicationDate = DateTime.Now
            };

            _context.HostApplications.Add(application);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}