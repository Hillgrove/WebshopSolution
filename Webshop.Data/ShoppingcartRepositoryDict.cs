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
        public class ShoppingCartRepositoryDict
        {
            private readonly Dictionary<int, (Product Product, int Quantity)> cart = new();

            public ShoppingCartRepositoryDict() { }

            public void AddProduct(Product product, int quantity = 1)
            {
                if (cart.ContainsKey(product.Id))
                {
                    cart[product.Id] = (product, cart[product.Id].Quantity + quantity);
                }
                else
                {
                    cart[product.Id] = (product, quantity);
                }
            }

            public async Task RemoveProductAsync(Product product, int quantity)
            {
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
                await Task.CompletedTask;
            }

            public async Task<decimal> GetTotalPriceAsync()
            {
                decimal totalPrice = cart.Sum(item => item.Value.Product.Price * item.Value.Quantity);
                return await Task.FromResult(totalPrice);
            }
        }

    }
}