using ComeStayHere.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComeStayHere.Models
{
    public class Payment
    {
        // Payment Table
        public int Id { get; set; }
        public int BookingId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Booking? Booking { get; set; }
    }
}
