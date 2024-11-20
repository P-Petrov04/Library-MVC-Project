using System.ComponentModel.DataAnnotations;

namespace Common.Entities
{
    public class Author
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }
        public string? Bio { get; set; }

        public ICollection<BookAuthor>? BookAuthors { get; set; }

    }
}
