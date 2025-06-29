using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Efood_Menu.Models;

namespace Efood_Menu.Repositories
{
    public interface IFoodItemRepository
    {
        Task<IEnumerable<FoodItem>> GetAllAsync();
        Task<FoodItem> GetByIdAsync(int id);
        Task AddAsync(FoodItem foodItem);
        Task UpdateAsync(FoodItem foodItem);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}