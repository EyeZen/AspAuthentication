using AspAuthentication.Auth.Enums;
using Microsoft.AspNetCore.Identity;

namespace AspAuthentication.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Role Role { get; set; }
    }
}
