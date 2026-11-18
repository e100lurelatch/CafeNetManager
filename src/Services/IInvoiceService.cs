using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: Thanh toán + lịch sử hóa đơn
    public interface IInvoiceService
    {
        /// <summary>
        /// Chốt thanh toán trong 1 lần: lưu hóa đơn, đánh dấu order đã thanh toán,
        /// tăng lượt dùng mã ưu đãi, trả bàn về Trống. Trả về hóa đơn đã có InvoiceId.
        /// </summary>
        Invoice Checkout(Invoice invoice);

        List<Invoice> GetInvoices(DateTime fromDate, DateTime toDate);
    }
}
