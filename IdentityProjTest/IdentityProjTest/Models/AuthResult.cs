namespace IdentityProjTest.Models
{
    public class AuthResult
    {
        public List<string>? Errors { get; set; }
        public string? Token { get; set; }
        public bool Result { get; set; }
        public string? Message { get; set; }
    }
}