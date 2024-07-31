
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
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultScheme = IdentityConstants.ApplicationScheme;
                x.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;

            })
                .AddBearerToken(IdentityConstants.BearerScheme)
                .AddCookie(IdentityConstants.ApplicationScheme);
               

            builder.Services.AddIdentityCore<User>()
                .AddEntityFrameworkStores<AppDBContext>()
                .AddApiEndpoints();

            builder.Services.AddDbContext<AppDBContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddControllers();

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

            app.UseHttpsRedirection();

            app.MapIdentityApi<User>();

            app.MapControllers();

            app.Run();
        }
    }
}
