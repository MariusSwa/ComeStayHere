using ComeStayHere.Data;
using ComeStayHere.Models.Enums;

namespace ComeStayHere.Models
{
    // Activity Log Table
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ActivityType ActivityType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ApplicationUser? User { get; set; }
    }
}
