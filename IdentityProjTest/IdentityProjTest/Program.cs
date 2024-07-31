
using IdentityProjTest.Database;
using IdentityProjTest.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace IdentityProjTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
                .AddCookie(IdentityConstants.ApplicationScheme)
                .AddBearerToken(IdentityConstants.BearerScheme);

            builder.Services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<AppDBContext>()
                .AddApiEndpoints();

            builder.Services.AddDbContext<AppDBContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.ApplyMigrations();
            }

            //добавим защищенную конечную точку
            app.MapGet("users/me", async (ClaimsPrincipal claims, AppDBContext context) =>
            {
                string userId = claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                return await context.Users.FindAsync(userId);
            })
            .RequireAuthorization();

            app.UseHttpsRedirection();

            app.MapIdentityApi<User>();

            app.Run();
        }
    }
}
