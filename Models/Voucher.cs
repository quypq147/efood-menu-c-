using System;
using System.ComponentModel.DataAnnotations;

namespace Efood_Menu.Models
{
    public class Voucher
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã giảm giá là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã không vượt quá 50 ký tự")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền giảm")]
        [Range(1000, 10000000, ErrorMessage = "Số tiền giảm phải từ 1.000 VNĐ trở lên")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Ngày hết hạn")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(250, ErrorMessage = "Mô tả không vượt quá 250 ký tự")]
        public string? Description { get; set; }
    }

}