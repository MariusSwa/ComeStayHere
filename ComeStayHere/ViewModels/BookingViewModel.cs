using System.ComponentModel.DataAnnotations;

namespace ComeStayHere.ViewModels
{
    public class BookingViewModel
    {
        public int PropertyId { get; set; }

        [Required]
        [Display(Name = "Check-in Date")]
        public DateTime CheckInDate { get; set; }

        [Required]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 20)]
        [Display(Name = "Number of Travelers")]
        public int NumberOfTravelers { get; set; } = 1;
    }
}