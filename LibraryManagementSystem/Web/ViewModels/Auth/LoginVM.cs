using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Auth
{
    public class LoginVM
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        public string? Url { get; set; }
    }
}
