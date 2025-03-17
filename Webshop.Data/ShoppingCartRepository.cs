using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data.Models;

namespace Webshop.Data
{
    public class ShoppingcartRepository : IShoppingcartRepository
    {
        // In-memory storage for shoppingcarts
        private readonly Dictionary<int, Shoppingcart> _shoppingcarts = new();

        public Task<Shoppingcart> GetShoppingcartByUserIdAsync(int userId)
        {
            _shoppingcarts.TryGetValue(userId, out var shoppingcart);
            return Task.FromResult(shoppingcart);
        }

        public async Task AddProductToCartAsync(int userId, int productId, int quantity)
        {
            // Find the user's shopping cart, or create one if it doesn't exist
            var shoppingcart = await GetShoppingcartByUserIdAsync(userId);
            if (shoppingcart == null)
            {
                shoppingcart = new Shoppingcart { UserId = userId, Products = new List<ProductInCart>() };
                _shoppingcarts[userId] = shoppingcart;
            }

            // Check if the product is already in the cart
            var productInCart = shoppingcart.Products.FirstOrDefault(p => p.ProductId == productId);
            if (productInCart != null)
            {
                // If the product is already in the cart, update the quantity
                productInCart.Quantity += quantity;
            }
            else
            {
                // If the product isn't in the cart, add a new entry
                shoppingcart.Products.Add(new ProductInCart { ProductId = productId, Quantity = quantity });
            }
        }
    }
}
