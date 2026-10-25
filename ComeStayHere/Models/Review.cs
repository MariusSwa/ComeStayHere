using ComeStayHere.Data;

namespace ComeStayHere.Models
{
    // Review Table
    public class Review
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string GuestId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public HostProperty? Property { get; set; }
        public ApplicationUser? Guest { get; set; }
    }
}
