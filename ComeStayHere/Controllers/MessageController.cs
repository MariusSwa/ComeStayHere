using ComeStayHere.Data;
using ComeStayHere.Models;
using ComeStayHere.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComeStayHere.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
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

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ReceiverId == user.Id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.ReceiverId == user.Id);

            if (message == null)
            {
                return NotFound();
            }

            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(
    int messageId,
    string content)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var originalMessage = await _context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == messageId &&
                    m.ReceiverId == user.Id);

            if (originalMessage == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = messageId });
            }

            var reply = new Message
            {
                SenderId = user.Id,
                ReceiverId = originalMessage.SenderId,
                Content = content,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(reply);

            var notification = new Notification
            {
                UserId = originalMessage.SenderId,
                Message = "You have received a new message.",
                Type = NotificationType.NewMessage,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Send(int propertyId)
        {
            var property = await _context.HostProperties
                .Include(p => p.Host)
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId &&
                    p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [Authorize(Roles = "Guest")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(
            int propertyId,
            string content)
                {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var property = await _context.HostProperties
                .FirstOrDefaultAsync(p =>
                    p.Id == propertyId &&
                    p.IsActive);

            if (property == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(
                    nameof(Send),
                    new { propertyId });
            }

            var message = new Message
            {
                SenderId = user.Id,
                ReceiverId = property.HostId,
                Content = content,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);

            var notification = new Notification
            {
                UserId = property.HostId,
                Message = "You have received a new message.",
                Type = NotificationType.NewMessage,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}