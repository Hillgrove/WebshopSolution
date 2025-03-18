using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data.Models;

namespace Webshop.Data
{
    public class ShoppingcartRepositoryDict
    {
        private readonly Dictionary<string, Dictionary<int, (Product Product, int Quantity)>> userCarts = new();

        // Fetch all products in the cart for a specific user
        public Task<Dictionary<int, (Product Product, int Quantity)>> GetAllAsync(string userEmail)
        {
            if (!userCarts.ContainsKey(userEmail))
            {
                return Task.FromResult(new Dictionary<int, (Product Product, int Quantity)>());
            }
            return Task.FromResult(new Dictionary<int, (Product Product, int Quantity)>(userCarts[userEmail]));
        }

        // Add product to a user's cart
        public async Task AddProductAsync(string userEmail, Product product, int quantity = 1)
        {
            if (!userCarts.ContainsKey(userEmail))
            {
                userCarts[userEmail] = new Dictionary<int, (Product Product, int Quantity)>();
            }

            var cart = userCarts[userEmail];

            if (cart.ContainsKey(product.Id))
            {
                cart[product.Id] = (product, cart[product.Id].Quantity + quantity);
            }
            else
            {
                cart[product.Id] = (product, quantity);
            }

            await Task.CompletedTask;
        }

        // Remove product from a user's cart
        public async Task RemoveProductAsync(string userEmail, Product product, int quantity)
        {
            if (userCarts.ContainsKey(userEmail))
            {
                var cart = userCarts[userEmail];

                if (cart.ContainsKey(product.Id))
                {
                    if (cart[product.Id].Quantity > quantity)
                    {
                        cart[product.Id] = (product, cart[product.Id].Quantity - quantity);
                    }
                    else
                    {
                        cart.Remove(product.Id); // Remove the product if quantity becomes 0 or less
                    }
                }
            }
            await Task.CompletedTask;
        }

        // Get the total price of the cart for a user
        public async Task<decimal> GetTotalPriceAsync(string userEmail)
        {
            if (!userCarts.ContainsKey(userEmail))
            {
                return await Task.FromResult(0m);
            }

            decimal totalPrice = userCarts[userEmail].Sum(item => item.Value.Product.PriceInOere * item.Value.Quantity);
            return await Task.FromResult(totalPrice);
        }
    }
}
