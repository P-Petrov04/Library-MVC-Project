using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Common.Entities
{
    public class User
    {
        public int Id { get; set; }
        public int RoleId { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        public string? Password { get; set; }


        public Role? Role { get; set; }
        public ICollection<Loan>? Loans { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
