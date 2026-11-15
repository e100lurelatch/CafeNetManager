using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CafeNetManager.Models;
using CafeNetManager.Services;

namespace CafeNetManager.ViewModels
{
    // Module 02 - Bảo: Quản lý ưu đãi (thêm / sửa / xóa / bật-tắt mã giảm giá)
    public class PromotionViewModel : ViewModelBase
    {
        private readonly IPromotionService _service;

        public ObservableCollection<Promotion> Promotions { get; } = new();

        private Promotion? _selectedPromotion;
        public Promotion? SelectedPromotion
        {
            get => _selectedPromotion;
            set
            {
                SetProperty(ref _selectedPromotion, value);
                if (value != null) FillForm(value);
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) Load(); }
        }

        // ===== Form nhập =====
        private int _editingId;
        public string FormTitle => _editingId == 0 ? "Thêm ưu đãi mới" : "Sửa ưu đãi";

        private string _code = string.Empty;
        public string Code { get => _code; set => SetProperty(ref _code, value); }

        private string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private int _discountTypeIndex;     // 0 = %, 1 = số tiền
        public int DiscountTypeIndex
        {
            get => _discountTypeIndex;
            set { if (SetProperty(ref _discountTypeIndex, value)) OnPropertyChanged(nameof(IsPercent)); }
        }
        public bool IsPercent => DiscountTypeIndex == 0;

        private string _discountValue = string.Empty;
        public string DiscountValue { get => _discountValue; set => SetProperty(ref _discountValue, value); }

        private string _minOrder = "0";
        public string MinOrder { get => _minOrder; set => SetProperty(ref _minOrder, value); }

        private string _maxDiscount = string.Empty;
        public string MaxDiscount { get => _maxDiscount; set => SetProperty(ref _maxDiscount, value); }

        private DateTime? _startDate = DateTime.Today;
        public DateTime? StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime? _endDate = DateTime.Today.AddMonths(1);
        public DateTime? EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private string _usageLimit = string.Empty;
        public string UsageLimit { get => _usageLimit; set => SetProperty(ref _usageLimit, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        public string SummaryText =>
            $"{Promotions.Count} ưu đãi  •  {Promotions.Count(p => p.StatusText == "Đang chạy")} đang chạy";

        public RelayCommand NewCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ToggleActiveCommand { get; }

        public PromotionViewModel() : this(AppServices.Promotions) { }

        public PromotionViewModel(IPromotionService service)
        {
            _service = service;
            NewCommand          = new RelayCommand(_ => ClearForm());
            SaveCommand         = new RelayCommand(_ => Save());
            DeleteCommand       = new RelayCommand(_ => Delete(), _ => SelectedPromotion != null);
            ToggleActiveCommand = new RelayCommand(_ => ToggleActive(), _ => SelectedPromotion != null);
            Load();
        }

        private void Load()
        {
            var keep = SelectedPromotion?.PromotionId;
            Promotions.Clear();
            var key = SearchText.Trim();
            foreach (var p in _service.GetAll().Where(p => key.Length == 0
                         || p.Code.Contains(key, StringComparison.OrdinalIgnoreCase)
                         || p.Name.Contains(key, StringComparison.OrdinalIgnoreCase)))
                Promotions.Add(p);
            _selectedPromotion = Promotions.FirstOrDefault(p => p.PromotionId == keep);
            OnPropertyChanged(nameof(SelectedPromotion));
            OnPropertyChanged(nameof(SummaryText));
        }

        private void FillForm(Promotion p)
        {
            _editingId = p.PromotionId;
            Code = p.Code;
            Name = p.Name;
            DiscountTypeIndex = (int)p.DiscountType;
            DiscountValue = p.DiscountValue.ToString("0.##", CultureInfo.InvariantCulture);
            MinOrder = p.MinOrderAmount.ToString("0", CultureInfo.InvariantCulture);
            MaxDiscount = p.MaxDiscount?.ToString("0", CultureInfo.InvariantCulture) ?? "";
            StartDate = p.StartDate;
            EndDate = p.EndDate;
            UsageLimit = p.UsageLimit?.ToString() ?? "";
            IsActive = p.IsActive;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void ClearForm()
        {
            _selectedPromotion = null;
            OnPropertyChanged(nameof(SelectedPromotion));
            _editingId = 0;
            Code = Name = DiscountValue = MaxDiscount = UsageLimit = string.Empty;
            MinOrder = "0";
            DiscountTypeIndex = 0;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
            IsActive = true;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void Save()
        {
            // ----- Kiểm tra dữ liệu -----
            var code = Code.Trim().ToUpperInvariant();
            if (code.Length is < 3 or > 20 || !code.All(char.IsLetterOrDigit))
            { Warn("Mã ưu đãi gồm 3-20 chữ cái/số, không dấu cách (VD: GIAM10)."); return; }
            if (string.IsNullOrWhiteSpace(Name)) { Warn("Nhập tên ưu đãi."); return; }

            if (!TryMoney(DiscountValue, out var value) || value <= 0)
            { Warn("Giá trị giảm phải là số lớn hơn 0."); return; }
            if (IsPercent && value > 100) { Warn("Giảm theo % chỉ từ 1 đến 100."); return; }

            if (!TryMoney(MinOrder, out var minOrder) || minOrder < 0)
            { Warn("Đơn tối thiểu phải là số từ 0 trở lên."); return; }

            decimal? maxDiscount = null;
            if (IsPercent && !string.IsNullOrWhiteSpace(MaxDiscount))
            {
                if (!TryMoney(MaxDiscount, out var m) || m <= 0) { Warn("Giảm tối đa phải là số lớn hơn 0 (hoặc để trống)."); return; }
                maxDiscount = m;
            }

            int? usageLimit = null;
            if (!string.IsNullOrWhiteSpace(UsageLimit))
            {
                if (!int.TryParse(UsageLimit, out var u) || u <= 0) { Warn("Số lượt dùng phải là số lớn hơn 0 (hoặc để trống)."); return; }
                usageLimit = u;
            }

            if (StartDate == null || EndDate == null) { Warn("Chọn ngày bắt đầu và kết thúc."); return; }
            if (EndDate.Value.Date < StartDate.Value.Date) { Warn("Ngày kết thúc phải sau ngày bắt đầu."); return; }

            var promo = new Promotion
            {
                PromotionId = _editingId,
                Code = code,
                Name = Name.Trim(),
                DiscountType = IsPercent ? DiscountType.Percent : DiscountType.Amount,
                DiscountValue = value,
                MinOrderAmount = minOrder,
                MaxDiscount = maxDiscount,
                StartDate = StartDate.Value.Date,
                EndDate = EndDate.Value.Date,
                UsageLimit = usageLimit,
                IsActive = IsActive
            };

            try
            {
                if (_editingId == 0) _editingId = _service.Add(promo);
                else _service.Update(promo);
                Code = code;
                Load();
                SelectedPromotion = Promotions.FirstOrDefault(p => p.PromotionId == _editingId);
            }
            catch (Exception ex) { Warn(ex.Message); }
        }

        private void Delete()
        {
            if (SelectedPromotion == null) return;
            if (MessageBox.Show($"Xóa ưu đãi {SelectedPromotion.Code}?", "Ưu đãi",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            try
            {
                _service.Delete(SelectedPromotion.PromotionId);
                ClearForm();
                Load();
            }
            catch (Exception ex) { Warn(ex.Message); }
        }

        private void ToggleActive()
        {
            if (SelectedPromotion == null) return;
            var p = SelectedPromotion;
            p.IsActive = !p.IsActive;
            try { _service.Update(p); } catch (Exception ex) { Warn(ex.Message); }
            Load();
            SelectedPromotion = Promotions.FirstOrDefault(x => x.PromotionId == p.PromotionId);
        }

        // Cho phép nhập 20000 / 20.000 / 20,000
        private static bool TryMoney(string text, out decimal value)
        {
            var t = text.Trim().Replace(".", "").Replace(",", "").Replace(" ", "");
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private static void Warn(string message) =>
            MessageBox.Show(message, "Ưu đãi", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
