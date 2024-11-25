using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Users
{
    public class RegisterUserVM
    {
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        //[MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        //[DataType(DataType.Password)]
        public string? Password { get; set; }
        public int RoleId { get; set; }
        public string? Role { get; set; }
    }
}
