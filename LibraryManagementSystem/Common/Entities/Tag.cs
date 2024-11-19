using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }

        public Category? Category { get; set; }
    }
}
