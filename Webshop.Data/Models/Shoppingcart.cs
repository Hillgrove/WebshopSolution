using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Data.Models
{
    public class Shoppingcart
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public Dictionary<int,(Product product, int quantity)>? Cart {  get; set; }
    }
}
