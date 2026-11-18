using System.IO;
using System.Text;
using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: in hóa đơn ra file .txt (thư mục Invoices cạnh file .exe)
    public static class InvoicePrinter
    {
        private const int Width = 42;

        public static string PrintToTxt(Invoice inv)
        {
            var sb = new StringBuilder();
            string Line(char c = '-') => new string(c, Width);
            string Row(string left, string right) =>
                left.Length + right.Length >= Width ? $"{left} {right}" : left + right.PadLeft(Width - left.Length);

            sb.AppendLine(Center("CAFE NET MANAGER"));
            sb.AppendLine(Center("HOA DON THANH TOAN"));
            sb.AppendLine(Line('='));
            sb.AppendLine($"So HD : {inv.InvoiceId}");
            sb.AppendLine($"Ban   : {inv.TableName}");
            sb.AppendLine($"Ngay  : {inv.PaidAt:dd/MM/yyyy HH:mm}");
            sb.AppendLine(Line());

            foreach (var i in inv.Items)
            {
                sb.AppendLine(i.DrinkName);
                if (!string.IsNullOrWhiteSpace(i.Note)) sb.AppendLine($"  ({i.Note})");
                sb.AppendLine(Row($"  {i.Quantity} x {i.UnitPrice:N0}", $"{i.LineTotal:N0}"));
            }

            sb.AppendLine(Line());
            sb.AppendLine(Row("Tam tinh:", $"{inv.SubTotal:N0}"));
            if (inv.DiscountAmount > 0)
                sb.AppendLine(Row($"Uu dai ({inv.PromotionCode}):", $"-{inv.DiscountAmount:N0}"));
            sb.AppendLine(Row("TONG CONG:", $"{inv.Total:N0} d"));
            sb.AppendLine(Line());
            sb.AppendLine(Row("Thanh toan:", inv.PaymentMethod));
            if (inv.CashReceived.HasValue)
            {
                sb.AppendLine(Row("Tien khach dua:", $"{inv.CashReceived:N0}"));
                sb.AppendLine(Row("Tien thoi lai:", $"{inv.ChangeAmount:N0}"));
            }
            if (!string.IsNullOrEmpty(inv.TransactionRef))
                sb.AppendLine(Row("Ma GD:", inv.TransactionRef));
            sb.AppendLine(Line('='));
            sb.AppendLine(Center("Cam on quy khach - Hen gap lai!"));

            var folder = Path.Combine(AppContext.BaseDirectory, "Invoices");
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, $"HD_{inv.InvoiceId}_{inv.PaidAt:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        private static string Center(string text) =>
            text.Length >= Width ? text : text.PadLeft((Width + text.Length) / 2);
    }
}
