namespace Skillo.Models
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? AccountType { get; set; }  // "offering" or "receiving" - which account type to use
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserLoginResponse? User { get; set; }
        public string? Token { get; set; }
    }

    public class UserLoginResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;  // "offering" or "receiving"
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImage { get; set; }
        public string? Bio { get; set; }
    }
}
