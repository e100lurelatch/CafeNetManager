using System.Data;
using CafeNetManager.DAL;
using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: thanh toán + hóa đơn trên SQL Server (bảng Invoices)
    public class SqlInvoiceService : IInvoiceService
    {
        private readonly DataProvider _db = DataProvider.Instance;

        public Invoice Checkout(Invoice inv)
        {
            _db.ExecuteInTransaction((conn, tran) =>
            {
                var orderId = DataProvider.Command(conn, tran,
                    "SELECT TOP 1 OrderId FROM Orders WHERE TableId = @t AND Status = 0",
                    new() { ["@t"] = inv.TableId }).ExecuteScalar();
                if (orderId == null || orderId == DBNull.Value)
                    throw new InvalidOperationException("Bàn này không có order nào chưa thanh toán.");

                // Tăng lượt dùng mã (chặn khi đã hết lượt)
                if (inv.PromotionId.HasValue)
                {
                    var ok = DataProvider.Command(conn, tran, @"
                        UPDATE Promotions SET UsedCount = UsedCount + 1
                        WHERE PromotionId = @p AND (UsageLimit IS NULL OR UsedCount < UsageLimit)",
                        new() { ["@p"] = inv.PromotionId }).ExecuteNonQuery();
                    if (ok == 0) throw new InvalidOperationException("Mã ưu đãi đã hết lượt sử dụng.");
                }

                inv.InvoiceId = Convert.ToInt32(DataProvider.Command(conn, tran, @"
                    INSERT INTO Invoices(OrderId, TableId, SubTotal, PromotionId, PromotionCode, DiscountAmount,
                                         Total, PaymentMethod, CashReceived, ChangeAmount, TransactionRef, PaidAt)
                    VALUES(@o, @t, @sub, @pid, @pcode, @disc, @total, @method, @cash, @change, @ref, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new()
                    {
                        ["@o"] = orderId, ["@t"] = inv.TableId, ["@sub"] = inv.SubTotal, ["@pid"] = inv.PromotionId,
                        ["@pcode"] = inv.PromotionCode, ["@disc"] = inv.DiscountAmount, ["@total"] = inv.Total,
                        ["@method"] = inv.PaymentMethod, ["@cash"] = inv.CashReceived, ["@change"] = inv.ChangeAmount,
                        ["@ref"] = inv.TransactionRef
                    }).ExecuteScalar());

                DataProvider.Command(conn, tran, @"
                    UPDATE Orders SET Status = 1 WHERE OrderId = @o;
                    UPDATE CafeTables SET Status = 0 WHERE TableId = @t;",
                    new() { ["@o"] = orderId, ["@t"] = inv.TableId }).ExecuteNonQuery();
            });

            inv.PaidAt = DateTime.Now;
            return inv;
        }

        public List<Invoice> GetInvoices(DateTime fromDate, DateTime toDate)
        {
            var dt = _db.ExecuteQuery(@"
                SELECT i.*, t.Name AS TableName FROM Invoices i
                JOIN CafeTables t ON t.TableId = i.TableId
                WHERE i.PaidAt >= @from AND i.PaidAt < @to
                ORDER BY i.PaidAt DESC",
                new() { ["@from"] = fromDate.Date, ["@to"] = toDate.Date.AddDays(1) });

            return dt.AsEnumerable().Select(r => new Invoice
            {
                InvoiceId = r.Field<int>("InvoiceId"),
                TableId = r.Field<int>("TableId"),
                TableName = r.Field<string>("TableName") ?? "",
                SubTotal = r.Field<decimal>("SubTotal"),
                PromotionId = r.Field<int?>("PromotionId"),
                PromotionCode = r.Field<string?>("PromotionCode"),
                DiscountAmount = r.Field<decimal>("DiscountAmount"),
                Total = r.Field<decimal>("Total"),
                PaymentMethod = r.Field<string>("PaymentMethod") ?? "",
                CashReceived = r.Field<decimal?>("CashReceived"),
                ChangeAmount = r.Field<decimal?>("ChangeAmount"),
                TransactionRef = r.Field<string?>("TransactionRef"),
                PaidAt = r.Field<DateTime>("PaidAt")
            }).ToList();
        }
    }
}
