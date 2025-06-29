using System.ComponentModel.DataAnnotations;

namespace Efood_Menu.Models
{
    public class Table
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TableNumber { get; set; } = string.Empty; // VD: A1, B2

        public int Capacity { get; set; } = 4; // Sức chứa

        public bool IsAvailable { get; set; } = true; // Dùng để tạm ẩn nếu đang bảo trì

        public ICollection<Reservation>? Reservations { get; set; }
    }
}
