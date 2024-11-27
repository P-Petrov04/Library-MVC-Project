using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Tags
{
    public class AddTagVM
    {
        [Required]
        public string? Name { get; set; }


        public int CategoryId { get; set; }
        public string? Category { get; set; }
    }
}
