﻿using Microsoft.AspNetCore.Mvc;

namespace Efood_Menu.Models
{
	public class CartItem
	{
		public int ProductId { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
        public string Image { get; set; }
    }
}
