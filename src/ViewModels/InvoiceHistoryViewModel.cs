using System.Collections.ObjectModel;
using System.Windows;
using CafeNetManager.Models;
using CafeNetManager.Services;

namespace CafeNetManager.ViewModels
{
    // Module 02 - Bảo: Lịch sử hóa đơn đã thanh toán
    public class InvoiceHistoryViewModel : ViewModelBase
    {
        private readonly IInvoiceService _service;

        public ObservableCollection<Invoice> Invoices { get; } = new();

        private DateTime? _fromDate = DateTime.Today;
        public DateTime? FromDate { get => _fromDate; set { if (SetProperty(ref _fromDate, value)) Load(); } }

        private DateTime? _toDate = DateTime.Today;
        public DateTime? ToDate { get => _toDate; set { if (SetProperty(ref _toDate, value)) Load(); } }

        public int InvoiceCount => Invoices.Count;
        public decimal TotalRevenue => Invoices.Sum(i => i.Total);
        public decimal TotalDiscount => Invoices.Sum(i => i.DiscountAmount);
        public decimal CashTotal => Invoices.Where(i => i.PaymentMethod == "Tiền mặt").Sum(i => i.Total);
        public decimal PayPalTotal => Invoices.Where(i => i.PaymentMethod == "PayPal").Sum(i => i.Total);

        public RelayCommand RefreshCommand { get; }
        public RelayCommand TodayCommand { get; }

        public InvoiceHistoryViewModel() : this(AppServices.Invoices) { }

        public InvoiceHistoryViewModel(IInvoiceService service)
        {
            _service = service;
            RefreshCommand = new RelayCommand(_ => Load());
            TodayCommand   = new RelayCommand(_ => { _fromDate = _toDate = DateTime.Today;
                                                     OnPropertyChanged(nameof(FromDate)); OnPropertyChanged(nameof(ToDate)); Load(); });
            Load();
            AppServices.InvoicesChanged += () => Application.Current?.Dispatcher.Invoke(Load);
        }

        private void Load()
        {
            Invoices.Clear();
            try
            {
                foreach (var i in _service.GetInvoices(FromDate ?? DateTime.Today, ToDate ?? DateTime.Today))
                    Invoices.Add(i);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được hóa đơn: " + ex.Message, "Hóa đơn",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            OnPropertyChanged(nameof(InvoiceCount));
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalDiscount));
            OnPropertyChanged(nameof(CashTotal));
            OnPropertyChanged(nameof(PayPalTotal));
        }
    }
}
