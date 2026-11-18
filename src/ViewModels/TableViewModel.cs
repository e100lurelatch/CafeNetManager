using System.Collections.ObjectModel;
using System.Windows;
using CafeNetManager.Models;
using CafeNetManager.Services;

namespace CafeNetManager.ViewModels
{
    // Module 02 - Bảo: Sơ đồ bàn + Gọi món + Chuyển bàn + mở màn hình Thanh toán
    public class TableViewModel : ViewModelBase
    {
        private readonly ITableService _service;

        public ObservableCollection<CafeTable> Tables { get; } = new();
        public ObservableCollection<Drink> Menu { get; } = new();
        public ObservableCollection<OrderItem> Items { get; } = new();
        public ObservableCollection<CafeTable> EmptyTables { get; } = new();   // để chọn khi chuyển bàn

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

        public int EmptyCount => Tables.Count(t => t.Status == TableStatus.Empty);
        public int OccupiedCount => Tables.Count(t => t.Status == TableStatus.Occupied);
        public int ReservedCount => Tables.Count(t => t.Status == TableStatus.Reserved);

        // ===== Commands =====
        public RelayCommand SelectTableCommand { get; }
        public RelayCommand AddItemCommand { get; }
        public RelayCommand IncreaseCommand { get; }
        public RelayCommand DecreaseCommand { get; }
        public RelayCommand RemoveItemCommand { get; }
        public RelayCommand ReserveCommand { get; }
        public RelayCommand TransferCommand { get; }
        public RelayCommand CheckoutCommand { get; }

        public TableViewModel() : this(AppServices.Tables) { }

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
            CheckoutCommand    = new RelayCommand(_ => OpenCheckout(), _ => HasSelectedTable && Items.Count > 0);

            foreach (var d in _service.GetMenu()) Menu.Add(d);
            LoadTables();

            // Màn hình Quản lý bàn thêm/sửa/xóa bàn -> tải lại sơ đồ
            AppServices.TablesChanged += () => Application.Current?.Dispatcher.Invoke(LoadTables);
        }

        // ===== Xử lý =====
        private void LoadTables()
        {
            var selectedId = SelectedTable?.TableId;
            Tables.Clear();
            foreach (var t in _service.GetTables()) Tables.Add(t);

            var again = Tables.FirstOrDefault(t => t.TableId == selectedId);
            if (again != null) SelectTable(again);
            else { SelectedTable = null; Items.Clear(); }
            RefreshSummary();
        }

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

            if (MessageBox.Show($"Chuyển {from.Name} sang {to.Name}?", "Chuyển bàn",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                _service.TransferTable(from.TableId, to.TableId);
                from.Status = TableStatus.Empty;
                to.Status = TableStatus.Occupied;
                SelectTable(to);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không chuyển được bàn: " + ex.Message, "Chuyển bàn",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Mở màn hình Thanh toán (ưu đãi, tiền mặt / PayPal, in hóa đơn)
        private void OpenCheckout()
        {
            if (SelectedTable == null || Items.Count == 0) return;
            var table = SelectedTable;

            var vm = new CheckoutViewModel(table, Items.ToList());
            var window = new Views.CheckoutWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            if (window.ShowDialog() != true || vm.Result == null) return;

            // Thanh toán xong: bàn về Trống
            table.Status = TableStatus.Empty;
            if (SelectedTable == table) Items.Clear();
            RefreshSummary();
            AppServices.NotifyInvoicesChanged();
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
