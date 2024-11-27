using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Categories
{
    public class EditCategoryVM
    {
        [Required]
        public string? Name { get; set; }
        public int Id { get; set; }
    }
}
