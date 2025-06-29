using Efood_Menu.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Efood_Menu.Repositories;

namespace Efood_Menu.Controllers
{
	public class HomeController : Controller
	{
		private readonly IFoodItemRepository _FoodItemRepository;

		public HomeController(IFoodItemRepository productRepository)
		{
			_FoodItemRepository = productRepository;
		}

		// Hiển thị trang chủ với danh sách sản phẩm
		public async Task<IActionResult> Index()
		{
			var products = await _FoodItemRepository.GetAllAsync();
			return View(products);
		}
		// Hiển thị trang Privacy
		[HttpGet]
		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
