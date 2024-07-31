using IdentityProjTest.Database;
using Microsoft.EntityFrameworkCore;

namespace IdentityProjTest.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using AppDBContext context = scope.ServiceProvider.GetRequiredService<AppDBContext>();

            context.Database.Migrate();
        }
    }
}
