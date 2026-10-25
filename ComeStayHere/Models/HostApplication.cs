using ComeStayHere.Data;
using ComeStayHere.Models.Enums;

namespace ComeStayHere.Models
{
    // Host Applications table
    public class HostApplication
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public ApplicationUser? User { get; set; }
        public HostProperty? Property { get; set; }
    }
}
