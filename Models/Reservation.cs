using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Efood_Menu.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Chọn ngày giờ đặt bàn")]
        [DataType(DataType.DateTime)]
        public DateTime ReservationDateTime { get; set; }

        [Range(1, 50, ErrorMessage = "Số lượng khách từ 1 đến 50")]
        public int NumberOfGuests { get; set; }
        [Required]
        public int TableId { get; set; }

        [ForeignKey("TableId")]
        public Table? Table { get; set; }

        public string? Note { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }


}
