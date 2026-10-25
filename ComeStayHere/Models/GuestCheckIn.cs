using ComeStayHere.Models.Enums;

namespace ComeStayHere.Models
{
    // Guest Check In Table
    public class GuestCheckIn
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public DateTime CheckInDate { get; set; }
        public CheckInStatus Status { get; set; } = CheckInStatus.Pending;
        public Booking? Booking { get; set; }
        public ICollection<GuestDocument> Documents { get; set; } = new List<GuestDocument>();
    }
}
