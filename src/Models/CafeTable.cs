using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CafeNetManager.Models
{
    // Module 02 - Bảo
    public enum TableStatus
    {
        Empty = 0,     // Trống
        Occupied = 1,  // Có khách
        Reserved = 2   // Đặt trước
    }

    public class CafeTable : INotifyPropertyChanged
    {
        private TableStatus _status;
        private bool _isSelected;

        public int TableId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public TableStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public string StatusText => Status switch
        {
            TableStatus.Occupied => "Có khách",
            TableStatus.Reserved => "Đặt trước",
            _ => "Trống"
        };

        // Dùng để tô viền bàn đang được chọn trên sơ đồ
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
