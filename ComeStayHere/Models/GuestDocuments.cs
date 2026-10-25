using ComeStayHere.Models.Enums;

namespace ComeStayHere.Models
{
    public class GuestDocument
    {
        public int Id { get; set; }
        public int GuestCheckInId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public GuestCheckIn? GuestCheckIn { get; set; }
    }
}
