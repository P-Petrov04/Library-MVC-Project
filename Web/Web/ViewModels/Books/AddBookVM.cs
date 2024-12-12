using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Required for file upload

namespace Web.ViewModels.Books
{
    public class AddBookVM
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string ISBN { get; set; }

        public string? Summary { get; set; }

        [Required]
        public int PublisherId { get; set; }

        [Required]
        public List<int> AuthorIds { get; set; } // Many-to-many relationship with Authors

        [Required]
        public List<int> CategoryIds { get; set; } // Many-to-many relationship with Categories

        [Required]
        public DateTime PublishedDate { get; set; } // To specify when the book was published

        public IFormFile? CoverImage { get; set; } // For file upload (image)
    }
}
