using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CafeNetManager.Views
{
    public enum MockPaymentResult { Cancelled, Success, Failed }

    // Code-behind chỉ vẽ giao diện + trả lựa chọn của người dùng, không có nghiệp vụ.
    public partial class MockPaymentWindow : Window
    {
        public MockPaymentResult Result { get; private set; } = MockPaymentResult.Cancelled;

        public MockPaymentWindow(string provider, string color, decimal amount, string orderId)
        {
            InitializeComponent();
            Title = $"Thanh toán {provider} (Mô phỏng)";
            txtProvider.Text = provider;
            header.Background = (Brush)new BrushConverter().ConvertFromString(color)!;
            txtAmount.Text = $"{amount:N0} đ";
            txtOrder.Text = "Mã đơn: " + orderId;
            DrawFakeQr(orderId);
        }

        // Vẽ ô vuông đen trắng giống mã QR (sinh ngẫu nhiên theo mã đơn)
        private void DrawFakeQr(string seedText)
        {
            const int n = 25;
            qrGrid.Rows = n; qrGrid.Columns = n;
            var rnd = new Random(seedText.GetHashCode());
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    bool black = IsFinder(r, c, n) ?? (rnd.NextDouble() < 0.45);
                    qrGrid.Children.Add(new Rectangle { Fill = black ? Brushes.Black : Brushes.White });
                }
        }

        // 3 ô định vị ở góc giống QR thật
        private static bool? IsFinder(int r, int c, int n)
        {
            foreach (var (r0, c0) in new[] { (0, 0), (0, n - 7), (n - 7, 0) })
            {
                int dr = r - r0, dc = c - c0;
                if (dr is >= -1 and <= 7 && dc is >= -1 and <= 7)
                {
                    if (dr is < 0 or > 6 || dc is < 0 or > 6) return false;      // viền trắng
                    int m = Math.Min(Math.Min(dr, 6 - dr), Math.Min(dc, 6 - dc));
                    return m != 1;                                               // đen - trắng - đen
                }
            }
            return null;
        }

        private void Success_Click(object sender, RoutedEventArgs e) { Result = MockPaymentResult.Success; DialogResult = true; }
        private void Failed_Click(object sender, RoutedEventArgs e)  { Result = MockPaymentResult.Failed;  DialogResult = true; }
        private void Cancel_Click(object sender, RoutedEventArgs e)  { Result = MockPaymentResult.Cancelled; DialogResult = false; }
    }
}
