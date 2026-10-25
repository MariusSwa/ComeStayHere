using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ComeStayHere.ViewModels
{
    public class HostApplicationViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue)]
        [Display(Name = "Price Per Night")]
        public decimal PricePerNight { get; set; }

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Property Type")]
        public string PropertyType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Property Image")]
        public IFormFile? PropertyImage { get; set; }
    }
}