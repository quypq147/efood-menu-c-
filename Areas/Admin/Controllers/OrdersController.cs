using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Efood_Menu.Models;

namespace Efood_Menu.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders.Include(o => o.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                            .Include(o => o.User)
                            .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.FoodItem)
                            .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderDate,UserId,TotalAmount,VoucherCode,DiscountApplied,Status,OrderType,TableNumber,ShippingAddress,Notes")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Admin/Orders/Edit/5
        
        
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderDate,UserId,TotalAmount,VoucherCode,DiscountApplied,Status,OrderType,TableNumber,ShippingAddress,Notes")] Order order)
        {
            if (id != order.Id)
                return BadRequest("Id không khớp.");

            if (!ModelState.IsValid)
                return BadRequest(string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            try
            {
                var oldOrder = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

                _context.Update(order);
                await _context.SaveChangesAsync();

                // Nếu trạng thái chuyển sang Done và là DineIn thì mở lại bàn
                if (order.OrderType == OrderType.DineIn && order.Status == "Done" && !string.IsNullOrEmpty(order.TableNumber))
                {
                    var table = await _context.Tables.FirstOrDefaultAsync(t => t.TableNumber == order.TableNumber);
                    if (table != null)
                    {
                        table.IsAvailable = true;
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Table {table.TableNumber} set to available!");
                    }
                    else
                    {
                        Console.WriteLine($"Table {order.TableNumber} not found!");
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                    return NotFound();
                else
                    throw;
            }
            return Ok();
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.Status = newStatus;

            // Nếu đơn hàng là ăn tại chỗ và trạng thái mới là "Done"
            if (order.OrderType == OrderType.DineIn && newStatus == "Done" && !string.IsNullOrEmpty(order.TableNumber))
            {
                var table = _context.Tables.FirstOrDefault(t => t.TableNumber == order.TableNumber);
                if (table != null)
                {
                    table.IsAvailable = true;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = orderId });
        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
