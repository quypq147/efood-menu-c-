using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Efood_Menu.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        // Navigation property
        public ICollection<FoodItem>? FoodItems { get; set; }
    }
}