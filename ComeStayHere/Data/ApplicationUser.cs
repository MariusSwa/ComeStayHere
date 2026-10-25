using Microsoft.AspNetCore.Identity;
namespace ComeStayHere.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string ProfilePictureUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
