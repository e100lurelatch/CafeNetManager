
using System;
using System.Windows;
using CafeNetManager.Services;
using CafeNetManager.ViewModels;

namespace CafeNetManager.Views
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly ChangePasswordViewModel viewModel = new ChangePasswordViewModel();
        private readonly AccountService accountService = new AccountService();

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

            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show("Vui lòng đăng nhập lại!");
                return;
            }

            try
            {
                bool thanhCong = accountService.DoiMatKhau(
                    UserSession.CurrentUser.UserId,
                    pbMatKhauCu.Password,
                    pbMatKhauMoi.Password);

                if (thanhCong)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!");
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Mật khẩu cũ không đúng hoặc tài khoản không còn hoạt động!");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Không thể đổi mật khẩu. Vui lòng kiểm tra kết nối SQL Server!");
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
