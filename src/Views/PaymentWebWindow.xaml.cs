using System.Windows;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;

namespace CafeNetManager.Views
{
    // Cửa sổ trình duyệt mở trang thanh toán online (không có logic nghiệp vụ):
    // mở link thanh toán, chặn lại khi cổng thanh toán chuyển về ReturnUrl và trả URL đó cho ViewModel.
    public partial class PaymentWebWindow : Window
    {
        private readonly string _paymentUrl;
        private readonly string _returnUrl;
        private readonly string _hint;

        /// <summary>URL cổng thanh toán trả về (chứa kết quả). null nếu người dùng đóng cửa sổ.</summary>
        public string? ResultUrl { get; private set; }

        public PaymentWebWindow(string paymentUrl, string returnUrl,
                           string title = "Thanh toán PayPal (Sandbox)",
                           string hint = "Đăng nhập bằng tài khoản PayPal sandbox loại Personal (xem trong Sandbox Accounts)",
                           string headerColor = "#003087")
        {
            InitializeComponent();
            _paymentUrl = paymentUrl;
            _returnUrl = returnUrl;
            _hint = hint;
            Title = title;
            header.Background = (Brush)new BrushConverter().ConvertFromString(headerColor)!;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.NavigationStarting += OnNavigationStarting;
                webView.CoreWebView2.NavigationCompleted += (_, _) =>
                    txtStatus.Text = _hint;
                webView.CoreWebView2.Navigate(_paymentUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được trình duyệt nhúng (WebView2).\n" + ex.Message,
                                Title, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (!e.Uri.StartsWith(_returnUrl, StringComparison.OrdinalIgnoreCase)) return;

            // Về tới ReturnUrl/RedirectUrl: không tải trang này (localhost không có server), chỉ lấy URL
            e.Cancel = true;
            ResultUrl = e.Uri;
            DialogResult = true;
        }
    }
}
