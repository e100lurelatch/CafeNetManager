
using CafeNetManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CafeNetManager.Views
{
    public partial class AccountView : UserControl
    {
        public AccountView()
        {
            InitializeComponent();
            DataContext = new AccountViewModel();
        }


        private void btnDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow window = new ChangePasswordWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }

    }
}
