using ComeStayHere.Data;
using ComeStayHere.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComeStayHere.Models
{
    // Booking Table
    public class Booking
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string GuestId { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfTravelers { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        // Set the status to pending on default
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public HostProperty? Property { get; set; }
        public ApplicationUser? Guest { get; set; }
        public Payment? Payment { get; set; }
        public GuestCheckIn? GuestCheckIn { get; set; }


    }
}
