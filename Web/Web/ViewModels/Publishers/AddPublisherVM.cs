using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Publishers
{
    public class AddPublisherVM
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string? Name { get; set; }
    }
}
