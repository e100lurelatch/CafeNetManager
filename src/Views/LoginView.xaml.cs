using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using CafeNetManager.ViewModels;

namespace CafeNetManager.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public bool Kiemtradangnhap(string ten, string pass)
        {
            if (ten.Equals("Admin") == true && pass.Equals("123456") == true)
                return true;
            return false;
        }

        private void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ten = pbDangNhap.Text;
                string pass = pbMatKhau.Password;
                if (Kiemtradangnhap(ten, pass))
                {
                    MessageBox.Show("Đăng nhập thành công !!");

                    CafeNetManager.MainWindow mainWindow = new CafeNetManager.MainWindow();
                    mainWindow.Show();
                    
                    this.Close();
                }
                else MessageBox.Show("Tên đăng nhập hoặc mật khẩu chưa đúng!!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
