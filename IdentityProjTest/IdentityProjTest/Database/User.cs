using Microsoft.AspNetCore.Identity;

namespace IdentityProjTest.Database
{
    public class User : IdentityUser
    {
        public string? Initials { get; set; }
    }
}
