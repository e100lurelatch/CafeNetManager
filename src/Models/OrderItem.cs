using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CafeNetManager.Models
{
    // Module 02 - Bảo: 1 dòng món trong order của bàn
    public class OrderItem : INotifyPropertyChanged
    {
        private int _quantity;

        public int DrinkId { get; set; }
        public string DrinkName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string Note { get; set; } = string.Empty;   // VD: ít đường, nhiều đá

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(LineTotal)); }
        }

        public decimal LineTotal => Quantity * UnitPrice;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
