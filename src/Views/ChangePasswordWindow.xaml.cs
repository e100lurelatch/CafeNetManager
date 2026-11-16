
using System.Windows;
using CafeNetManager.ViewModels;

namespace CafeNetManager.Views
{
    public partial class ChangePasswordWindow : Window
    {
        ChangePasswordViewModel viewModel = new ChangePasswordViewModel();

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            string ketQua = viewModel.KiemTraMatKhau(
                pbMatKhauCu.Password,
                pbMatKhauMoi.Password,
                pbXacNhan.Password);

            if (ketQua != "")
            {
                MessageBox.Show(ketQua);
                return;
            }

            MessageBox.Show("Thông tin hợp lệ! Chưa cập nhật mật khẩu thật.");
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
