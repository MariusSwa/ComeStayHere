using ComeStayHere.Data;

namespace ComeStayHere.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
        public ApplicationUser? Sender { get; set; }
        public ApplicationUser? Receiver { get; set; }
    }
}
