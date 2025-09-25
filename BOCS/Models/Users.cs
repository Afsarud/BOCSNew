using Microsoft.AspNetCore.Identity;

namespace BOCS.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string? Role { get; set; }   // Nullable করে দিলাম
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Default value
        public string? CurrentSessionId { get; set; } // For single active session support
    }
}
