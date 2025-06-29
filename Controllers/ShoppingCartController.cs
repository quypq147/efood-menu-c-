using Efood_Menu.Extensions;
using Efood_Menu.Models;
using Efood_Menu.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Previewer;
using System.IO;

namespace Efood_Menu.Controllers
{
	
	public class ShoppingCartController : Controller
	{
		private readonly IFoodItemRepository _foodItemRepository;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFoodItemRepository productRepository)
		{
			_foodItemRepository = productRepository;
			_context = context;
			_userManager = userManager;
		}

		public IActionResult Index()
		{
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
			return View(cart);
		}
        [HttpGet]
        public IActionResult Checkout()
        {
            var tables = _context.Tables.ToList();
            ViewBag.Tables = tables;
            return View(new Order());
        }

        [HttpPost]
		public async Task<IActionResult> Checkout(Order order, int? NumberOfGuests, DateTime? ReservationDateTime, string? TableNumber)
		{
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            

            order.UserId = user?.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            order.Status = "Pending";

            order.OrderItems = cart.Items.Select(i => new OrderItem
            {
                FoodItemId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList();

            // Nếu là ăn tại chỗ thì tạo Reservation
            if (order.OrderType == OrderType.DineIn)
            {
                if (NumberOfGuests == null || ReservationDateTime == null || string.IsNullOrEmpty(TableNumber))
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin đặt bàn.");
                    var tables = _context.Tables.ToList();
                    ViewBag.Tables = tables;
                    return View(order);
                }

                // Tìm TableId từ TableNumber
                var table = _context.Tables.FirstOrDefault(t => t.TableNumber == TableNumber);
                if (table == null)
                {
                    ModelState.AddModelError("", "Bàn không hợp lệ.");
                    var tables = _context.Tables.ToList();
                    ViewBag.Tables = tables;
                    return View(order);
                }

                var reservation = new Reservation
                {
                    FullName = order.User?.FullName ?? "",
                    PhoneNumber = order.User?.PhoneNumber ?? "",
                    ReservationDateTime = ReservationDateTime.Value,
                    NumberOfGuests = NumberOfGuests.Value,
                    TableId = table.Id,
                    Note = order.Notes,
                    UserId = user?.Id
                };

                _context.Reservations.Add(reservation);
                // Có thể cập nhật trạng thái bàn nếu cần
                table.IsAvailable = false;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");
            // Fetch order with items and food info for the receipt
            var orderWithDetails = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == order.Id);

            ViewBag.Order = orderWithDetails;

            return View("OrderCompleted", order.Id);
        }

		public async Task<IActionResult> AddToCart(int productId, int quantity)
		{
			var product = await GetProductFromDatabase(productId);
			if (product == null)
			{
				return NotFound();
			}

			var cartItem = new CartItem
			{
				ProductId = productId,
				Name = product.Name,
				Price = product.Price,
				Quantity = quantity,
				Image = product.ImageUrl ?? "default-image-url.jpg" // Thay thế bằng URL ảnh mặc định nếu không có ảnh
            };

			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
			cart.AddItem(cartItem);
			HttpContext.Session.SetObjectAsJson("Cart", cart);
            int cartCount = cart.Items.Sum(i => i.Quantity);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount });
            }
            return RedirectToAction("Index");
		}

		public IActionResult RemoveFromCart(int productId)
		{
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

			if (cart != null)
			{
				cart.RemoveItem(productId);
				HttpContext.Session.SetObjectAsJson("Cart", cart);
			}

			return RedirectToAction("Index");
		}

		private async Task<FoodItem> GetProductFromDatabase(int productId)
		{
			return await _foodItemRepository.GetByIdAsync(productId);
		}
        public async Task<IActionResult> DownloadReceipt(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            var pdfBytes = GenerateOrderReceiptPdf(order);

            var fileName = $"PhieuDatMon_{order.Id}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [Obsolete]
        private byte[] GenerateOrderReceiptPdf(Order order)
        {
            // Đường dẫn vật lý tuyệt đối tới file PNG
            var iconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "icons", "restaunrant.png");

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A5);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("PHIẾU ĐẶT MÓN")
                                .FontSize(20).Bold().FontColor("#FF9671");
                            col.Item().Text($"Mã đơn hàng: #{order.Id}").FontSize(12).FontColor("#888");
                            col.Item().Text($"Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}").FontSize(12);
                        });
                        // Chỉ nhúng ảnh nếu file tồn tại
                        if (System.IO.File.Exists(iconPath))
                        {
                            row.ConstantItem(60).Image(iconPath, ImageScaling.FitArea);
                        }
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(6);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Người đặt: {order.User?.FullName ?? "-"}");
                            row.RelativeItem().AlignRight().Text($"Loại đơn: {order.OrderType}");
                        });

                        if (order.OrderType == OrderType.DineIn && !string.IsNullOrEmpty(order.TableNumber))
                            col.Item().Text($"Số bàn: {order.TableNumber}");

                        if (order.OrderType == OrderType.Delivery && !string.IsNullOrEmpty(order.ShippingAddress))
                            col.Item().Text($"Địa chỉ giao hàng: {order.ShippingAddress}");

                        if (!string.IsNullOrWhiteSpace(order.Notes))
                            col.Item().Text($"Ghi chú: {order.Notes}");

                        col.Item().PaddingVertical(8).LineHorizontal(0.8f).LineColor("#FF9671");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Món").Bold();
                                header.Cell().AlignRight().Text("SL").Bold();
                                header.Cell().AlignRight().Text("Đơn giá").Bold();
                                header.Cell().AlignRight().Text("Thành tiền").Bold();
                            });

                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().Text(item.FoodItem?.Name ?? "-");
                                table.Cell().AlignRight().Text(item.Quantity.ToString());
                                table.Cell().AlignRight().Text($"{item.UnitPrice:N0} ₫");
                                table.Cell().AlignRight().Text($"{(item.UnitPrice * item.Quantity):N0} ₫");
                            }

                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(3).AlignRight().Text("Tổng cộng").Bold();
                                footer.Cell().AlignRight().Text($"{order.TotalAmount:N0} ₫").Bold().FontColor("#FF9671");
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text("Cảm ơn quý khách!").FontSize(12).FontColor("#FF9671");
                });
            });

            return pdf.GeneratePdf();
        }
    }
}
