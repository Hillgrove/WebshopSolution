﻿namespace Webshop.Shared.DTOs
{
    public class CartUpdateDto
    {
        public int ProductId { get; set; }
        public int Delta { get; set; } // +X or -X
    }

}
