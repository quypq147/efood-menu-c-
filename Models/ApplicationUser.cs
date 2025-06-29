using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations; // Add this namespace



namespace Efood_Menu.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự")]
        public string FullName { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [Range(10, 100, ErrorMessage = "Tuổi phải từ 10 đến 100")]
        public string? Age { get; set; }
    }


    public static class SD
	{
		public const string Role_Customer = "Khách hàng";
		public const string Role_Admin = "Quản trị viên";
		public const string Role_Employee = "Nhân viên";
		public const string Role_Company = "Chủ cửa hàng";
    }
}
