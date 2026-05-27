using System.ComponentModel.DataAnnotations.Schema;

namespace APIEccomerce.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        
        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;

        
        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}