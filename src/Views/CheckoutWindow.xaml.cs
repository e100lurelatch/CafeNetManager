using System.Windows;
using CafeNetManager.ViewModels;

namespace CafeNetManager.Views
{
    // Code-behind chỉ nối sự kiện "đóng cửa sổ" của ViewModel, không có nghiệp vụ.
    public partial class CheckoutWindow : Window
    {
        public CheckoutWindow()
        {
            InitializeComponent();
            DataContextChanged += (_, e) =>
            {
                if (e.NewValue is CheckoutViewModel vm)
                    vm.RequestClose += ok => DialogResult = ok;
            };
        }
    }
}
