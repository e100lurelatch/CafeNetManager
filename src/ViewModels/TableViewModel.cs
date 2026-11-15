using System.Collections.ObjectModel;
using System.Windows;
using CafeNetManager.Models;
using CafeNetManager.Services;

namespace CafeNetManager.ViewModels
{
    // Module 02 - Bảo: Sơ đồ bàn + Gọi món + Chuyển bàn
    public class TableViewModel : ViewModelBase
    {
        private readonly ITableService _service;

        public ObservableCollection<CafeTable> Tables { get; } = new();
        public ObservableCollection<Drink> Menu { get; } = new();
        public ObservableCollection<OrderItem> Items { get; } = new();

        // Danh sách bàn trống để chọn khi chuyển bàn
        public ObservableCollection<CafeTable> EmptyTables { get; } = new();

        // ===== Thuộc tính bind ra giao diện =====
        private CafeTable? _selectedTable;
        public CafeTable? SelectedTable
        {
            get => _selectedTable;
            private set
            {
                if (_selectedTable != null) _selectedTable.IsSelected = false;
                SetProperty(ref _selectedTable, value);
                if (_selectedTable != null) _selectedTable.IsSelected = true;
                OnPropertyChanged(nameof(HasSelectedTable));
                OnPropertyChanged(nameof(SelectedTableTitle));
            }
        }
        public bool HasSelectedTable => SelectedTable != null;
        public string SelectedTableTitle =>
            SelectedTable == null ? "Chọn một bàn trên sơ đồ"
                                  : $"{SelectedTable.Name} ({SelectedTable.StatusText})";

        private Drink? _selectedDrink;
        public Drink? SelectedDrink { get => _selectedDrink; set => SetProperty(ref _selectedDrink, value); }

        private int _quantity = 1;
        public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        private string _note = string.Empty;
        public string Note { get => _note; set => SetProperty(ref _note, value); }

        private CafeTable? _transferTarget;
        public CafeTable? TransferTarget { get => _transferTarget; set => SetProperty(ref _transferTarget, value); }

        public decimal Total => Items.Sum(i => i.LineTotal);

        // Thống kê nhỏ trên đầu sơ đồ
        public int EmptyCount => Tables.Count(t => t.Status == TableStatus.Empty);
        public int OccupiedCount => Tables.Count(t => t.Status == TableStatus.Occupied);
        public int ReservedCount => Tables.Count(t => t.Status == TableStatus.Reserved);

        // ===== Commands (bind vào Button.Command) =====
        public RelayCommand SelectTableCommand { get; }
        public RelayCommand AddItemCommand { get; }
        public RelayCommand IncreaseCommand { get; }
        public RelayCommand DecreaseCommand { get; }
        public RelayCommand RemoveItemCommand { get; }
        public RelayCommand ReserveCommand { get; }
        public RelayCommand TransferCommand { get; }
        public RelayCommand CheckoutCommand { get; }

        // Constructor không tham số để XAML tạo được ViewModel.
        // Dùng SQL nếu kết nối được, không thì tự chuyển sang dữ liệu giả để vẫn làm giao diện được.
        public TableViewModel() : this(CreateService()) { }

        private static ITableService CreateService()
        {
            // Đang mở trong designer của Visual Studio -> dùng dữ liệu giả
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return new MockTableService();

            try
            {
                var error = CafeNetManager.DAL.DataProvider.Instance.TestConnection();
                if (error == null) return new SqlTableService();

                MessageBox.Show("Không kết nối được SQL Server, đang dùng dữ liệu giả.\n\n"
                              + "Kiểm tra: đã chạy database/CafeNetManager.sql chưa, "
                              + "Server= trong App.config đúng chưa.\n\nChi tiết: " + error,
                                "Kết nối CSDL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cấu hình CSDL: " + ex.Message, "Kết nối CSDL",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return new MockTableService();
        }

        public TableViewModel(ITableService service)
        {
            _service = service;

            SelectTableCommand = new RelayCommand(p => SelectTable(p as CafeTable));
            AddItemCommand     = new RelayCommand(_ => AddItem(), _ => HasSelectedTable && SelectedDrink != null && Quantity > 0);
            IncreaseCommand    = new RelayCommand(p => ChangeQuantity(p as OrderItem, +1));
            DecreaseCommand    = new RelayCommand(p => ChangeQuantity(p as OrderItem, -1));
            RemoveItemCommand  = new RelayCommand(p => RemoveItem(p as OrderItem));
            ReserveCommand     = new RelayCommand(_ => ToggleReserve(),
                                    _ => SelectedTable != null && SelectedTable.Status != TableStatus.Occupied);
            TransferCommand    = new RelayCommand(_ => Transfer(),
                                    _ => SelectedTable?.Status == TableStatus.Occupied && TransferTarget != null);
            CheckoutCommand    = new RelayCommand(_ => Checkout(), _ => HasSelectedTable && Items.Count > 0);

            foreach (var t in _service.GetTables()) Tables.Add(t);
            foreach (var d in _service.GetMenu()) Menu.Add(d);
            RefreshSummary();
        }

        // ===== Xử lý =====
        private void SelectTable(CafeTable? table)
        {
            if (table == null) return;
            SelectedTable = table;

            Items.Clear();
            foreach (var i in _service.GetOrderItems(table.TableId)) Items.Add(i);

            ResetInput();
            RefreshSummary();
        }

        private void AddItem()
        {
            if (SelectedTable == null || SelectedDrink == null || Quantity <= 0) return;
            var note = Note.Trim();

            // Cùng món + cùng ghi chú thì cộng dồn số lượng
            var existing = Items.FirstOrDefault(i => i.DrinkId == SelectedDrink.DrinkId && i.Note == note);
            if (existing != null)
                existing.Quantity += Quantity;
            else
                Items.Add(new OrderItem
                {
                    DrinkId = SelectedDrink.DrinkId,
                    DrinkName = SelectedDrink.Name,
                    UnitPrice = SelectedDrink.Price,
                    Quantity = Quantity,
                    Note = note
                });

            // Bàn trống/đặt trước có món đầu tiên -> chuyển sang Có khách
            if (SelectedTable.Status != TableStatus.Occupied)
                SetTableStatus(SelectedTable, TableStatus.Occupied);

            SaveOrder();
            ResetInput();
        }

        private void ChangeQuantity(OrderItem? item, int delta)
        {
            if (item == null) return;
            if (item.Quantity + delta <= 0) { RemoveItem(item); return; }
            item.Quantity += delta;
            SaveOrder();
        }

        private void RemoveItem(OrderItem? item)
        {
            if (item == null || SelectedTable == null) return;
            Items.Remove(item);

            // Xóa hết món -> trả bàn về Trống
            if (Items.Count == 0)
            {
                _service.ClearTable(SelectedTable.TableId);
                SetTableStatus(SelectedTable, TableStatus.Empty);
            }
            else SaveOrder();

            RefreshSummary();
        }

        private void ToggleReserve()
        {
            if (SelectedTable == null) return;
            var newStatus = SelectedTable.Status == TableStatus.Reserved ? TableStatus.Empty : TableStatus.Reserved;
            SetTableStatus(SelectedTable, newStatus);
        }

        private void Transfer()
        {
            if (SelectedTable == null || TransferTarget == null) return;
            var from = SelectedTable;
            var to = TransferTarget;

            var ok = MessageBox.Show($"Chuyển {from.Name} sang {to.Name}?", "Chuyển bàn",
                                     MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ok != MessageBoxResult.Yes) return;

            _service.TransferTable(from.TableId, to.TableId);
            from.Status = TableStatus.Empty;
            to.Status = TableStatus.Occupied;

            SelectTable(to);   // mở luôn bàn mới
        }

        // Thanh toán đơn giản: hiện tổng tiền rồi trả bàn về Trống.
        // Phần giảm giá / in hóa đơn / lưu doanh thu nối vào đây sau.
        private void Checkout()
        {
            if (SelectedTable == null) return;
            var ok = MessageBox.Show($"Thanh toán {SelectedTable.Name}\nTổng tiền: {Total:N0} đ",
                                     "Thanh toán", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (ok != MessageBoxResult.OK) return;

            _service.CheckoutTable(SelectedTable.TableId);
            SetTableStatus(SelectedTable, TableStatus.Empty);
            Items.Clear();
            RefreshSummary();
        }

        // ===== Hàm phụ =====
        private void SetTableStatus(CafeTable table, TableStatus status)
        {
            table.Status = status;
            _service.UpdateTableStatus(table.TableId, status);
            RefreshSummary();
        }

        private void SaveOrder()
        {
            if (SelectedTable == null) return;
            _service.SaveOrderItems(SelectedTable.TableId, Items);
            OnPropertyChanged(nameof(Total));
        }

        private void ResetInput()
        {
            SelectedDrink = null;
            Quantity = 1;
            Note = string.Empty;
        }

        private void RefreshSummary()
        {
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(EmptyCount));
            OnPropertyChanged(nameof(OccupiedCount));
            OnPropertyChanged(nameof(ReservedCount));
            OnPropertyChanged(nameof(SelectedTableTitle));

            EmptyTables.Clear();
            foreach (var t in Tables.Where(t => t.Status == TableStatus.Empty && t != SelectedTable))
                EmptyTables.Add(t);
            TransferTarget = null;
        }
    }
}
