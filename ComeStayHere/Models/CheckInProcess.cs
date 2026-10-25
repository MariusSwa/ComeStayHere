namespace ComeStayHere.Models
{
    public class CheckInProcess
    {
        // Check in process table
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresDocument { get; set; }
        public string RequiredDocumentType { get; set; } = string.Empty;
        public HostProperty? Property { get; set; }
    }
}
