using Microsoft.AspNetCore.Mvc;
using Efood_Menu.Models; // Sửa lại namespace cho đúng
using System.Linq;

namespace Efood_Menu.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.OrderCount = _context.Orders.Count();
            ViewBag.FoodItemCount = _context.FoodItems.Count();
            ViewBag.UserCount = _context.Users.Count();
            ViewBag.CategoryCount = _context.Categories.Count();
            // Lấy tổng doanh thu (chỉ tính đơn đã hoàn thành, ví dụ Status = "Done")
            ViewBag.TotalRevenue = _context.Orders
                .Where(o => o.Status == "Done")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // Lấy doanh thu theo OrderType
            ViewBag.RevenueByOrderType = _context.Orders
                .Where(o => o.Status == "Done")
                .GroupBy(o => o.OrderType)
                .Select(g => new { OrderType = g.Key.ToString(), Revenue = g.Sum(x => x.TotalAmount) })
                .ToList();
            // Ví dụ: Lấy dữ liệu cho pie chart
            ViewBag.OrderStatusData = _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return View();
        }
    }
}
