using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Efood_Menu.Models;
using Efood_Menu.Repositories;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Efood_Menu.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FoodItemsController : Controller
    {
        private readonly IFoodItemRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public FoodItemsController(IFoodItemRepository repository, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
        }

        // GET: FoodItems
        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, string search)
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories;

            var foodItems = await _repository.GetAllAsync();
            if (categoryId.HasValue)
                foodItems = foodItems.Where(f => f.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                foodItems = foodItems.Where(f => f.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            return View(foodItems);
        }

        // GET: FoodItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _repository.GetByIdAsync(id.Value);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // GET: FoodItems/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: FoodItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,Description,Price,CategoryId,ImageUrl")] FoodItem foodItem,
            IFormFile ImageFile,
            List<IFormFile> AdditionalImages)
        {
            const long maxFileSize = 2 * 1024 * 1024;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            // 1. Kiểm tra các trường khác (trừ ảnh)
            if (foodItem == null || string.IsNullOrWhiteSpace(foodItem.Name) || foodItem.Price <= 0 || foodItem.CategoryId == 0)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin món ăn.");
            }

            // 2. Kiểm tra file ảnh
            if (ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Ảnh món ăn chính là bắt buộc.");
            }
            else
            {
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "File phải là hình ảnh (.jpg, .jpeg, .png, .gif).");
                }
                else if (ImageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("ImageFile", "File ảnh quá lớn. Vui lòng chọn file nhỏ hơn 2MB.");
                }
            }

            // 3. Nếu có lỗi, trả lại view, KHÔNG lưu ảnh
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        Console.WriteLine($"ModelState Error - {key}: {error.ErrorMessage}");
                    }
                }
                var categories = await _categoryRepository.GetAllAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }

            // 4. Nếu hợp lệ, mới lưu ảnh và gán ImageUrl
            foodItem.ImageUrl = await SaveImageAsync(ImageFile);

            // 5. Lưu ảnh phụ nếu có
            if (AdditionalImages != null && AdditionalImages.Any())
            {
                if (foodItem.Images == null)
                    foodItem.Images = new List<FoodImage>();
                foreach (var file in AdditionalImages)
                {
                    var url = await SaveImageAsync(file);
                    foodItem.Images.Add(new FoodImage { ImageUrl = url });
                }
            }

            try
            {
                await _repository.AddAsync(foodItem);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Create] Exception: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Có lỗi khi lưu dữ liệu.");
                var categories = await _categoryRepository.GetAllAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }

        }

        // GET: FoodItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _repository.GetByIdAsync(id.Value);
            if (foodItem == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", foodItem.CategoryId);
            return View(foodItem);
        }

        // POST: FoodItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,Name,Description,Price,CategoryId,ImageUrl")] FoodItem foodItem,
    IFormFile ImageFile,
    List<IFormFile> AdditionalImages,
    bool RemoveMainImage,
    string RemoveImageIds)
        {
            if (id != foodItem.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAllAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }

            // Get the tracked entity
            var oldItem = await _repository.GetByIdAsync(id);
            if (oldItem == null)
                return NotFound();

            // Update properties
            oldItem.Name = foodItem.Name;
            oldItem.Description = foodItem.Description;
            oldItem.Price = foodItem.Price;
            oldItem.CategoryId = foodItem.CategoryId;

            // Handle main image
            if (RemoveMainImage)
            {
                oldItem.ImageUrl = null;
            }
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                oldItem.ImageUrl = await SaveImageAsync(ImageFile);
            }
            else if (!string.IsNullOrWhiteSpace(foodItem.ImageUrl))
            {
                oldItem.ImageUrl = foodItem.ImageUrl;
            }
            // else keep old image

            // Remove additional images if requested
            if (!string.IsNullOrEmpty(RemoveImageIds) && oldItem.Images != null)
            {
                var idsToRemove = RemoveImageIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(idStr => int.TryParse(idStr, out var idVal) ? idVal : (int?)null)
                    .Where(idVal => idVal.HasValue)
                    .Select(idVal => idVal.Value)
                    .ToList();
                oldItem.Images = oldItem.Images.Where(img => !idsToRemove.Contains(img.Id)).ToList();
            }

            // Add new additional images
            if (AdditionalImages != null && AdditionalImages.Any())
            {
                if (oldItem.Images == null)
                    oldItem.Images = new List<FoodImage>();
                foreach (var file in AdditionalImages)
                {
                    var url = await SaveImageAsync(file);
                    oldItem.Images.Add(new FoodImage { ImageUrl = url });
                }
            }

            try
            {
                await _repository.UpdateAsync(oldItem);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Edit] Exception: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Có lỗi khi lưu dữ liệu.");
                var categories = await _categoryRepository.GetAllAsync();
                ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", foodItem.CategoryId);
                return View(foodItem);
            }
        }

        // GET: FoodItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _repository.GetByIdAsync(id.Value);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // POST: FoodItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        private async Task<string> SaveImageAsync(IFormFile imageFile, string subFolder = "foods")
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var uploadsFolder = Path.Combine("wwwroot/images", subFolder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Return the relative path for storing in the database
            return $"/images/{subFolder}/{uniqueFileName}";
        }


    }
}
