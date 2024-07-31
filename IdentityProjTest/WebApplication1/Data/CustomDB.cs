using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data
{
    public class CustomDB :IdentityDbContext<IdentityUser>
    {
        public CustomDB(DbContextOptions options) : base(options) { }
    }
}
