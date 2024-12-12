using System.ComponentModel.DataAnnotations;

namespace Common.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string? Title { get; set; }

        [Required]
        [StringLength(20)]
        public string? ISBN { get; set; }

        public string? Summary { get; set; }

        public int PublisherId { get; set; }

        public DateTime PublishedDate { get; set; }

        [StringLength(500)] 
        public string? CoverImagePath { get; set; }

        public Publisher? Publisher { get; set; }
        public ICollection<BookAuthor>? BookAuthors { get; set; }
        public ICollection<BookCategory>? BookCategories { get; set; }
        public ICollection<Loan>? Loans { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
