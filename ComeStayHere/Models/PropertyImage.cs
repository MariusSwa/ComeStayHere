namespace ComeStayHere.Models
{
    // Property Table
    public class PropertyImage
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public HostProperty? Property { get; set; }
    }
}
