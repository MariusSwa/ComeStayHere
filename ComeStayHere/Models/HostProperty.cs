using ComeStayHere.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComeStayHere.Models
{
    // Host Property Table
    public class HostProperty
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CleaningFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceFee { get; set; }

        public string HostId { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public ApplicationUser? Host { get; set; }

        public ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public CheckInProcess? CheckInProcess { get; set; }
    }
}
