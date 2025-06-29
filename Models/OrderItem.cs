using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Efood_Menu.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [ForeignKey("FoodItem")]
        public int FoodItemId { get; set; }
        public FoodItem? FoodItem { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int Quantity { get; set; }

        [Range(1000, 100000000)]
        public decimal UnitPrice { get; set; }
    }

}
