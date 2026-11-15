using System.Collections.ObjectModel;
using System.Windows;
using CafeNetManager.Models;
using CafeNetManager.Services;

namespace CafeNetManager.ViewModels
{
    // Module 02 - Bảo: Quản lý bàn ăn (thêm / sửa / xóa)
    public class TableManagerViewModel : ViewModelBase
    {
        private readonly ITableService _service;

        public ObservableCollection<CafeTable> Tables { get; } = new();

        private CafeTable? _selectedTable;
        public CafeTable? SelectedTable
        {
            get => _selectedTable;
            set
            {
                SetProperty(ref _selectedTable, value);
                if (value == null) return;
                _editingId = value.TableId;
                EditName = value.Name;
                EditCapacity = value.Capacity.ToString();
                OnPropertyChanged(nameof(FormTitle));
            }
        }

        // ===== Form nhập =====
        private int _editingId;     // 0 = đang thêm bàn mới
        public string FormTitle => _editingId == 0 ? "Thêm bàn mới" : $"Sửa bàn (mã {_editingId})";

        private string _editName = string.Empty;
        public string EditName { get => _editName; set => SetProperty(ref _editName, value); }

        private string _editCapacity = "4";
        public string EditCapacity { get => _editCapacity; set => SetProperty(ref _editCapacity, value); }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) Load(); }
        }

        public string SummaryText => $"{Tables.Count} bàn  •  {Tables.Sum(t => t.Capacity)} chỗ ngồi";

        public RelayCommand NewCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public TableManagerViewModel() : this(AppServices.Tables) { }

        public TableManagerViewModel(ITableService service)
        {
            _service = service;
            NewCommand    = new RelayCommand(_ => NewTable());
            SaveCommand   = new RelayCommand(_ => Save(), _ => !string.IsNullOrWhiteSpace(EditName));
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedTable != null);

            Load();
            AppServices.TablesChanged += () => Application.Current?.Dispatcher.Invoke(Load);
        }

        private void Load()
        {
            var keep = SelectedTable?.TableId;
            Tables.Clear();
            foreach (var t in _service.GetTables()
                         .Where(t => string.IsNullOrWhiteSpace(SearchText)
                                  || t.Name.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase)))
                Tables.Add(t);
            _selectedTable = Tables.FirstOrDefault(t => t.TableId == keep);
            OnPropertyChanged(nameof(SelectedTable));
            OnPropertyChanged(nameof(SummaryText));
        }

        private void NewTable()
        {
            _selectedTable = null;
            OnPropertyChanged(nameof(SelectedTable));
            _editingId = 0;
            EditName = $"Bàn {(_service.GetTables().Count + 1)}";
            EditCapacity = "4";
            OnPropertyChanged(nameof(FormTitle));
        }

        private void Save()
        {
            var name = EditName.Trim();
            if (name.Length is 0 or > 50)
            {
                Warn("Tên bàn không được trống và tối đa 50 ký tự.");
                return;
            }
            if (!int.TryParse(EditCapacity, out var capacity) || capacity < 1 || capacity > 30)
            {
                Warn("Số chỗ phải là số từ 1 đến 30.");
                return;
            }

            try
            {
                var table = new CafeTable { TableId = _editingId, Name = name, Capacity = capacity };
                if (_editingId == 0) _editingId = _service.AddTable(table);
                else _service.UpdateTable(table);

                OnPropertyChanged(nameof(FormTitle));
                AppServices.NotifyTablesChanged();     // sơ đồ bàn + màn hình này tự tải lại
                SelectedTable = Tables.FirstOrDefault(t => t.TableId == _editingId);
            }
            catch (Exception ex) { Warn(ex.Message); }
        }

        private void Delete()
        {
            if (SelectedTable == null) return;
            if (MessageBox.Show($"Xóa {SelectedTable.Name}?", "Quản lý bàn",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            try
            {
                _service.DeleteTable(SelectedTable.TableId);
                NewTable();
                AppServices.NotifyTablesChanged();
            }
            catch (Exception ex) { Warn(ex.Message); }
        }

        private static void Warn(string message) =>
            MessageBox.Show(message, "Quản lý bàn", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
