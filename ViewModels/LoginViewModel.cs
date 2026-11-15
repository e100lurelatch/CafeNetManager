using CafeNetManager.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CafeNetManager.ViewModels
{
    public class LoginViewModel
    {
        // Chứa dữ liệu tạm thời để View liên kết (Binding) qua
        public User LoginAccount { get; set; } = new User();

        // Hàm xử lý logic khi bấm nút đăng nhập
        public void HandleLogin()
        {
            if (LoginAccount.Username == "admin" && LoginAccount.Password == "123456")
            {
                MessageBox.Show("Đăng nhập thành công quán cà phê!");
                // Logic chuyển sang màn hình chính ở đây
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
            }
        }
    }
}
