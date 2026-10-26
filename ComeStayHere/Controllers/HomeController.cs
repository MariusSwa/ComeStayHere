using ComeStayHere.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComeStayHere.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var properties = await _context.HostProperties
                .Include(p => p.PropertyImages)
                .Where(p => p.IsActive)
                .Take(6)
                .ToListAsync();

            return View(properties);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}