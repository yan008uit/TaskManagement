using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Models.DTOs
{
    public class RegisterRequest
    {
        [Required (ErrorMessage = "Username is required to create an account.")]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
        [StringLength(50,  MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters")]
        public string Username { get; set; } = string.Empty;
        
        [Required (ErrorMessage = "Email is required to create an account.")]
        [EmailAddress (ErrorMessage = "Email is not valid.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 100 characters")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required to create an account.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 200 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required to log in.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username is between 4 and 50 characters")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required to log in.")]
        [StringLength(200, MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
