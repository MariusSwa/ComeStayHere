using ComeStayHere.Data;

namespace ComeStayHere.Models
{
    // WishlistItem table
    public class WishlistItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public ApplicationUser? User { get; set; }
        public HostProperty? Property { get; set; }
    }
}
