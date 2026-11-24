
using System.Windows;
using System.Windows.Controls;
using CafeNetManager.ViewModels;
using CafeNetManager.Services;

namespace CafeNetManager.Views
{
    public partial class AccountView : UserControl
    {
        public AccountView()
        {
            InitializeComponent();

            AccountViewModel viewModel = new AccountViewModel();

            if (UserSession.CurrentUser != null)
            {
                viewModel.NapTaiKhoan(UserSession.CurrentUser);
            }

            DataContext = viewModel;
        }

        private void btnDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow window = new ChangePasswordWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}
