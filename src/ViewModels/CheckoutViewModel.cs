using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using CafeNetManager.Models;
using CafeNetManager.Services;

namespace CafeNetManager.ViewModels
{
    // Module 02 - Bảo: Màn hình Thanh toán
    // Áp mã ưu đãi -> chọn Tiền mặt (tính tiền thối) hoặc PayPal -> lưu hóa đơn -> in hóa đơn .txt
    public class CheckoutViewModel : ViewModelBase
    {
        private readonly CafeTable _table;

        public string TableName => _table.Name;
        public List<OrderItem> Items { get; }
        public decimal SubTotal { get; }

        /// <summary>Hóa đơn sau khi thanh toán thành công (null nếu chưa / hủy).</summary>
        public Invoice? Result { get; private set; }

        /// <summary>ViewModel yêu cầu đóng cửa sổ (true = thanh toán xong).</summary>
        public event Action<bool>? RequestClose;

        // ===== Ưu đãi =====
        public ObservableCollection<Promotion> AvailablePromotions { get; } = new();

        private string _promoCode = string.Empty;
        public string PromoCode { get => _promoCode; set => SetProperty(ref _promoCode, value); }

        private Promotion? _appliedPromotion;
        public Promotion? AppliedPromotion
        {
            get => _appliedPromotion;
            private set
            {
                SetProperty(ref _appliedPromotion, value);
                OnPropertyChanged(nameof(HasPromotion));
                RefreshMoney();
            }
        }
        public bool HasPromotion => AppliedPromotion != null;

        // Chọn nhanh mã trong danh sách
        private Promotion? _selectedPromotion;
        public Promotion? SelectedPromotion
        {
            get => _selectedPromotion;
            set
            {
                if (!SetProperty(ref _selectedPromotion, value) || value == null) return;
                PromoCode = value.Code;
                ApplyCode();
            }
        }

        private string _promoMessage = string.Empty;
        public string PromoMessage { get => _promoMessage; set => SetProperty(ref _promoMessage, value); }

        private bool _promoError;
        public bool PromoError { get => _promoError; set => SetProperty(ref _promoError, value); }

        public decimal DiscountAmount => AppliedPromotion?.CalculateDiscount(SubTotal) ?? 0;
        public decimal Total => SubTotal - DiscountAmount;

        // PayPal không hỗ trợ VND nên quy đổi sang USD theo tỷ giá trong App.config
        private readonly PayPalService _paypal = new();
        public string PayPalAmountText =>
            $"≈ {_paypal.ConvertFromVnd(Total).ToString("0.00", CultureInfo.InvariantCulture)} {_paypal.Currency} (tỷ giá {_paypal.VndPerUnit:N0}đ = 1 {_paypal.Currency})";

        // ===== Hình thức thanh toán =====
        private bool _isCash = true;
        public bool IsCash
        {
            get => _isCash;
            set { if (SetProperty(ref _isCash, value)) OnPropertyChanged(nameof(IsPayPal)); }
        }
        public bool IsPayPal
        {
            get => !_isCash;
            set => IsCash = !value;
        }

        private string _cashReceivedText = string.Empty;
        public string CashReceivedText
        {
            get => _cashReceivedText;
            set { if (SetProperty(ref _cashReceivedText, value)) RefreshMoney(); }
        }

        public decimal? CashReceived
        {
            get
            {
                var digits = new string(CashReceivedText.Where(char.IsDigit).ToArray());
                return decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var v) ? v : null;
            }
        }

        public decimal? ChangeAmount => CashReceived.HasValue ? CashReceived.Value - Total : null;
        public string ChangeText => ChangeAmount switch
        {
            null => "",
            < 0m => $"Còn thiếu {-ChangeAmount.Value:N0}đ",
            _ => $"Tiền thối lại: {ChangeAmount.Value:N0}đ"
        };
        public bool ChangeNotEnough => ChangeAmount < 0;

        // Nút chọn nhanh tiền khách đưa
        public ObservableCollection<decimal> QuickCashOptions { get; } = new();

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        // ===== Commands =====
        public RelayCommand ApplyCodeCommand { get; }
        public RelayCommand RemovePromotionCommand { get; }
        public RelayCommand QuickCashCommand { get; }
        public RelayCommand ConfirmCommand { get; }
        public RelayCommand CancelCommand { get; }

        public CheckoutViewModel(CafeTable table, List<OrderItem> items)
        {
            _table = table;
            Items = items;
            SubTotal = items.Sum(i => i.LineTotal);

            ApplyCodeCommand       = new RelayCommand(_ => ApplyCode(), _ => !string.IsNullOrWhiteSpace(PromoCode));
            RemovePromotionCommand = new RelayCommand(_ => RemovePromotion(), _ => HasPromotion);
            QuickCashCommand       = new RelayCommand(p => { if (p is decimal v) CashReceivedText = v.ToString("N0"); });
            ConfirmCommand         = new RelayCommand(async _ => await ConfirmAsync(), _ => !IsBusy);
            CancelCommand          = new RelayCommand(_ => RequestClose?.Invoke(false), _ => !IsBusy);

            // Chỉ liệt kê mã đang dùng được cho đơn này
            var now = DateTime.Now;
            foreach (var p in AppServices.Promotions.GetAll().Where(p => p.CheckApplicable(SubTotal, now) == null))
                AvailablePromotions.Add(p);

            RefreshMoney();
        }

        // ===== Ưu đãi =====
        private void ApplyCode()
        {
            var code = PromoCode.Trim().ToUpperInvariant();
            if (code.Length == 0) return;

            var promo = AppServices.Promotions.GetByCode(code);
            if (promo == null) { ShowPromo($"Không tìm thấy mã \"{code}\".", true); return; }

            var error = promo.CheckApplicable(SubTotal, DateTime.Now);
            if (error != null) { ShowPromo(error, true); return; }

            AppliedPromotion = promo;
            ShowPromo($"Đã áp dụng {promo.Code}: {promo.DiscountText} (-{DiscountAmount:N0}đ)", false);
        }

        private void RemovePromotion()
        {
            AppliedPromotion = null;
            _selectedPromotion = null;
            OnPropertyChanged(nameof(SelectedPromotion));
            PromoCode = string.Empty;
            ShowPromo("", false);
        }

        private void ShowPromo(string message, bool isError)
        {
            PromoMessage = message;
            PromoError = isError;
        }

        private void RefreshMoney()
        {
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(PayPalAmountText));
            OnPropertyChanged(nameof(ChangeAmount));
            OnPropertyChanged(nameof(ChangeText));
            OnPropertyChanged(nameof(ChangeNotEnough));

            // Gợi ý: đúng số tiền, làm tròn lên 10k, 50k, 100k, 200k, 500k
            QuickCashOptions.Clear();
            foreach (var v in new[] { Total, RoundUp(Total, 10000), RoundUp(Total, 50000),
                                      RoundUp(Total, 100000), RoundUp(Total, 200000), RoundUp(Total, 500000) }
                              .Where(v => v > 0).Distinct().OrderBy(v => v))
                QuickCashOptions.Add(v);
        }

        private static decimal RoundUp(decimal value, decimal step) => Math.Ceiling(value / step) * step;

        // ===== Thanh toán =====
        private async Task ConfirmAsync()
        {
            var invoice = new Invoice
            {
                TableId = _table.TableId,
                TableName = _table.Name,
                SubTotal = SubTotal,
                PromotionId = AppliedPromotion?.PromotionId,
                PromotionCode = AppliedPromotion?.Code,
                DiscountAmount = DiscountAmount,
                Total = Total,
                Items = Items
            };

            if (IsCash)
            {
                if (CashReceived == null || ChangeAmount < 0)
                {
                    MessageBox.Show("Nhập số tiền khách đưa (phải lớn hơn hoặc bằng tổng tiền).", "Thanh toán",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                invoice.PaymentMethod = "Tiền mặt";
                invoice.CashReceived = CashReceived;
                invoice.ChangeAmount = ChangeAmount;
            }
            else
            {
                if (Total < 1000)
                {
                    MessageBox.Show("Đơn dưới 1.000đ, hãy chọn Tiền mặt.", "Thanh toán",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var paid = await PayWithPayPalAsync();
                if (paid == null) return;        // hủy / thất bại: giữ nguyên cửa sổ để chọn lại
                invoice.PaymentMethod = "PayPal";
                invoice.TransactionRef = paid;
            }

            try
            {
                IsBusy = true;
                Result = AppServices.Invoices.Checkout(invoice);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lưu được hóa đơn: " + ex.Message, "Thanh toán",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally { IsBusy = false; }

            // In hóa đơn ra file .txt
            string? path = null;
            try { path = InvoicePrinter.PrintToTxt(Result); } catch { /* lỗi in không ảnh hưởng thanh toán */ }

            var msg = $"Thanh toán thành công!\n\n{Result.TableName}  -  Hóa đơn #{Result.InvoiceId}\n"
                    + $"Tổng tiền: {Result.Total:N0}đ ({Result.PaymentMethod})";
            if (Result.ChangeAmount > 0) msg += $"\nTiền thối lại: {Result.ChangeAmount:N0}đ";

            if (path != null)
            {
                if (MessageBox.Show(msg + "\n\nMở hóa đơn vừa in?", "Thanh toán", MessageBoxButton.YesNo,
                                    MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
                }
            }
            else MessageBox.Show(msg, "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Information);

            RequestClose?.Invoke(true);
        }

        /// <summary>
        /// Thanh toán qua PayPal Sandbox: tạo order -> khách đăng nhập tài khoản sandbox và duyệt
        /// -> app gọi capture để chốt tiền. Trả về mã giao dịch nếu thành công, null nếu hủy / lỗi.
        /// </summary>
        private async Task<string?> PayWithPayPalAsync()
        {
            var referenceId = $"CAFE{_table.TableId}T{DateTime.Now:yyyyMMddHHmmss}";

            // Chế độ mô phỏng (demo khi không có mạng / chưa có mã)
            if (string.Equals(System.Configuration.ConfigurationManager.AppSettings["PayPal_Mode"], "Simulate",
                              StringComparison.OrdinalIgnoreCase))
            {
                var mock = new Views.MockPaymentWindow("PayPal", "#003087", Total, referenceId)
                {
                    Owner = Application.Current.MainWindow
                };
                mock.ShowDialog();
                if (mock.Result == Views.MockPaymentResult.Success) return referenceId;
                if (mock.Result == Views.MockPaymentResult.Failed)
                    MessageBox.Show("Thanh toán PayPal không thành công.", "PayPal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            if (!_paypal.IsConfigured)
            {
                MessageBox.Show("Chưa cấu hình PayPal Sandbox.\n\nĐiền PayPal_ClientId và PayPal_Secret (lấy ở developer.paypal.com "
                              + "→ Apps & Credentials, chế độ Sandbox) vào file src/PayPal.local.config.",
                                "PayPal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            try
            {
                // 1. Tạo order
                IsBusy = true;
                var (orderId, approveUrl) = await _paypal.CreateOrderAsync(Total, referenceId, $"Thanh toan {_table.Name}");
                IsBusy = false;

                // 2. Khách đăng nhập tài khoản PayPal sandbox (personal) và bấm thanh toán
                var window = new Views.PaymentWebWindow(approveUrl, _paypal.ReturnUrl) { Owner = Application.Current.MainWindow };
                if (window.ShowDialog() != true || window.ResultUrl == null) return null;      // đóng cửa sổ

                var (cancelled, returnedOrderId) = _paypal.ParseReturnUrl(window.ResultUrl);
                if (cancelled)
                {
                    MessageBox.Show("Khách đã hủy thanh toán PayPal.", "PayPal", MessageBoxButton.OK, MessageBoxImage.Information);
                    return null;
                }
                if (returnedOrderId != null && returnedOrderId != orderId)
                {
                    MessageBox.Show("Mã order PayPal trả về không khớp. Giao dịch bị từ chối.", "PayPal",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                // 3. Capture: server PayPal xác nhận và chốt tiền (không tin vào URL trả về)
                IsBusy = true;
                var result = await _paypal.CaptureOrderAsync(orderId);
                if (!result.IsSuccess)
                {
                    MessageBox.Show($"Thanh toán PayPal chưa hoàn tất (trạng thái {result.Status}).", "PayPal",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
                return string.IsNullOrEmpty(result.CaptureId) ? orderId : result.CaptureId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán PayPal.\n\n" + ex.Message, "PayPal", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            finally { IsBusy = false; }
        }
    }
}
