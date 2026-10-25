using ComeStayHere.Data;
using ComeStayHere.Models.Enums;

namespace ComeStayHere.Models
{
    public class Notification
    {
        // Notification Table
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
