using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Efood_Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Efood_Menu.Repositories
{
    public class EFFoodItemRepository : IFoodItemRepository
    {
        private readonly ApplicationDbContext _context;

        public EFFoodItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FoodItem>> GetAllAsync()
        {
            return await _context.FoodItems.Include(f => f.Category).ToListAsync();
        }

        public async Task<FoodItem> GetByIdAsync(int id)
        {
            return await _context.FoodItems.Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(FoodItem foodItem)
        {
            _context.FoodItems.Add(foodItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FoodItem foodItem)
        {
            _context.FoodItems.Update(foodItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem != null)
            {
                _context.FoodItems.Remove(foodItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.FoodItems.AnyAsync(f => f.Id == id);
        }
    }
}