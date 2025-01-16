using Microsoft.AspNetCore.Identity;

namespace IdentityProjTest.Models
{
    public class User : IdentityUser
    {
        public string? Initials { get; set; }
        public int Age { get; set; }
    }
}
