using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class Publisher
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public ICollection<Book>? Books { get; set; }
    }
}
