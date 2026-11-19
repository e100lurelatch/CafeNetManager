
using System;
using System.Windows;
using CafeNetManager.Services;

namespace CafeNetManager.Views
{
    public partial class LoginView : Window
    {
        private readonly AccountService accountService = new AccountService();

        public LoginView()
        {
            InitializeComponent();
        }

        private void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ten = pbDangNhap.Text.Trim();
                string pass = pbMatKhau.Password;

                var taiKhoan = accountService.XacThucDangNhap(ten, pass);


                if (taiKhoan != null)
                {
                    UserSession.CurrentUser = taiKhoan;

                    MessageBox.Show("Đăng nhập thành công!");

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    this.Close();
                }

                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đăng nhập: " + ex.Message);
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
