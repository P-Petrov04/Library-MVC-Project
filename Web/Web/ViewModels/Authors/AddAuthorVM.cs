using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Authors
{
    public class AddAuthorVM
    {
        [Required]
        public string Name { get; set; }
    }
}
