using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Books
{
    public class EditBookVM
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string ISBN { get; set; }

        public string? Summary { get; set; }

        [Required]
        public int PublisherId { get; set; }

        [Required]
        public List<int> AuthorIds { get; set; }

        [Required]
        public List<int> CategoryIds { get; set; }

        public IFormFile? CoverImage { get; set; } 
        public string? CoverImagePath { get; set; }

        public DateTime PublishedDate { get; set; }
    }
}
