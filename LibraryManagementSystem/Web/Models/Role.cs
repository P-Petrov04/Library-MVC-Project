using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class Role
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }
    }
}
