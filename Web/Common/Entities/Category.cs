using System.ComponentModel.DataAnnotations;

namespace Common.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }

        public ICollection<Tag>? Tag { get; set; }
        public ICollection<BookCategory>? BookCategories { get; set; }
    }
}
