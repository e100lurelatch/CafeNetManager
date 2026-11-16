namespace CafeNetManager.Models
{
    // Module 02 - Bảo: Hóa đơn sau khi thanh toán
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }                    // tiền món
        public int? PromotionId { get; set; }
        public string? PromotionCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }                       // khách phải trả
        public string PaymentMethod { get; set; } = "Tiền mặt";  // Tiền mặt / PayPal
        public decimal? CashReceived { get; set; }               // tiền khách đưa (tiền mặt)
        public decimal? ChangeAmount { get; set; }               // tiền thối lại
        public string? TransactionRef { get; set; }              // mã giao dịch PayPal
        public DateTime PaidAt { get; set; } = DateTime.Now;

        // Chỉ dùng khi in hóa đơn (không lưu trong bảng Invoices)
        public List<OrderItem> Items { get; set; } = new();

        public string PaidAtText => PaidAt.ToString("dd/MM/yyyy HH:mm");
    }
}
