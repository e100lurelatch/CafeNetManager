using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: hóa đơn dữ liệu giả (chỉ nằm trong RAM)
    public class MockInvoiceService : IInvoiceService
    {
        private readonly MockTableService _tables;
        private readonly MockPromotionService _promotions;
        private readonly List<Invoice> _invoices = new();

        public MockInvoiceService(MockTableService tables, MockPromotionService promotions)
        {
            _tables = tables;
            _promotions = promotions;
        }

        public Invoice Checkout(Invoice invoice)
        {
            if (invoice.PromotionId.HasValue) _promotions.IncreaseUsage(invoice.PromotionId.Value);
            invoice.InvoiceId = _invoices.Count + 1;
            invoice.PaidAt = DateTime.Now;
            _invoices.Add(invoice);
            _tables.CheckoutTable(invoice.TableId);
            return invoice;
        }

        public List<Invoice> GetInvoices(DateTime fromDate, DateTime toDate) =>
            _invoices.Where(i => i.PaidAt.Date >= fromDate.Date && i.PaidAt.Date <= toDate.Date)
                     .OrderByDescending(i => i.PaidAt).ToList();
    }
}
