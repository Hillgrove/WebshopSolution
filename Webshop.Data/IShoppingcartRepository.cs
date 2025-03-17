using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data.Models;

namespace Webshop.Data
{
    public interface IShoppingcartRepository
    {
        Task<Shoppingcart> GetShoppingcartByUserIdAsync(int userId);
        Task AddProductToCartAsync(int userId, int productId, int quantity);
    }
}
