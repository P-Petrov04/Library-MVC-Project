using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Authors
{
    public class EditAuthorVM
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

    }
}
