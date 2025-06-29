using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Efood_Menu.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(150)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Nhập Giá")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        
        // Main image for the food item
        [DisplayName("Hình ảnh")]
        public string? ImageUrl { get; set; } // Ảnh đại diện

        // Navigation property for multiple images (additional images)
        public ICollection<FoodImage>? Images { get; set; }

        [ForeignKey("Category")]
        [DisplayName("Mã danh mục")]
        public int CategoryId { get; set; }

        [DisplayName("Danh mục")]
        public Category? Category { get; set; }
    }
}