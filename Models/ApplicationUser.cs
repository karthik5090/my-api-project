using Microsoft.AspNetCore.Identity;

namespace ForgotPasswordApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
    }
}
