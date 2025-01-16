using IdentityProjTest.Configurations;
using IdentityProjTest.Models;
using IdentityProjTest.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityProjTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        //private readonly JwtConfig _jwtConfig;

        public AuthenticationController(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            if (ModelState.IsValid)
            {
                var user_exist = await _userManager.FindByEmailAsync(requestDto.Email);

                if (user_exist is not null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Email already exist"
                        }
                    });
                }

                var new_user = new User()
                {
                    Email = requestDto.Email,
                    UserName = requestDto.Email,
                };

                var is_created = await _userManager.CreateAsync(new_user, requestDto.Password);

                if (is_created.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(new_user);
                    await _userManager.AddToRoleAsync(new_user, "Team");

                    return Ok(new AuthResult()
                    {
                        Result = true,
                        Message = $"Please confirm you email with the code that you have received {code}"
                    });
                }

                return BadRequest(new AuthResult()
                {
                    Errors = new List<string>()
                    {
                        "Server error"
                    },
                    Result = false
                });
            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                    {
                        "Invalid payload"
                    },
                Result = false
            });
        }

        [HttpPost]
        [Route("EmailVerification")]
        public async Task<IActionResult> EmailVerification(string? email, string? code)
        {
            if (email is null || code is null)
            {
                return BadRequest("Invalid payload");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return BadRequest("Invalid payload");
            }

            var isVerified = await _userManager.ConfirmEmailAsync(user, code);

            if (isVerified.Succeeded)
            {
                return Ok(new { message = "email confirmed" });
            }

            return BadRequest("something went wrong");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequest)
        {
            if (ModelState.IsValid)
            {
                var existing_user = await _userManager.FindByEmailAsync(loginRequest.Email);

                if (existing_user is null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid payload"
                        },
                        Result = false
                    }); 
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existing_user, loginRequest.Password);

                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid credentials"
                        },
                        Result = false
                    });
                }

                var roles = await _userManager.GetRolesAsync(existing_user);

                var jwtToken = GenerateJwtToken(existing_user, roles);

                return Ok(new AuthResult()
                {
                    Token = jwtToken,
                    Result = true
                });
            }

            return BadRequest(new AuthResult()
            {
                Errors = new List<string>()
                  {
                    "Invalid payload"
                  },
                Result = false
            });
        }

        private string GenerateJwtToken(User user, IList<string> roles)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:SecretKey").Value);

            var claimsList = new List<Claim>();
            claimsList.Add(new Claim("Id", user.Id));
            claimsList.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Email));
            claimsList.Add(new Claim(JwtRegisteredClaimNames.Email, value: user.Email));
            claimsList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claimsList.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()));

            claimsList.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, user.Id)));

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claimsList),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }
    }
}
