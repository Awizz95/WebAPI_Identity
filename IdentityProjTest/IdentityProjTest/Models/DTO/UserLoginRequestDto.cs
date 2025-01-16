namespace IdentityProjTest.Models.DTO
{
    public class UserLoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}