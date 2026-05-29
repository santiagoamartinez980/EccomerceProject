using System.ComponentModel.DataAnnotations.Schema;

namespace APIEccomerce.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "COP";
        public string? PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public Order Order { get; set; } = null!;
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Approved = 1,
        Declined = 2,
        Voided = 3,
        Error = 4
    }
}
