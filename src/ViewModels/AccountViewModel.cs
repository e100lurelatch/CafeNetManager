using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CafeNetManager.Models;
using CafeNetManager.Services;

namespace CafeNetManager.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {

        private readonly AccountService accountService = new AccountService();
        private int? userId = null;

        public void NapTaiKhoan(AccountProfile taiKhoan)
        {
            userId = taiKhoan.UserId;
            Username = taiKhoan.Username;
            HoTen = taiKhoan.FullName;
            Email = taiKhoan.Email;
            SoDienThoai = taiKhoan.Phone;
            VaiTro = taiKhoan.Role;
        }


        // Dữ liệu mẫu. Chưa có bảng tài khoản và phiên đăng nhập thực tế.
        private string _username = "Admin";
        public string Username
        {
            get => _username;
            private set => SetProperty(ref _username, value);
        }

        private string _hoTen = "";
        public string HoTen
        {
            get => _hoTen;
            set => SetProperty(ref _hoTen, value);
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _soDienThoai = "";
        public string SoDienThoai
        {
            get => _soDienThoai;
            set => SetProperty(ref _soDienThoai, value);
        }

        private string _vaiTro = "";
        public string VaiTro
        {
            get => _vaiTro;
            private set => SetProperty(ref _vaiTro, value);
        }

        private bool _dangChinhSua;
        public bool DangChinhSua
        {
            get => _dangChinhSua;
            set => SetProperty(ref _dangChinhSua, value);
        }

        private string hoTenCu = "";
        private string emailCu = "";
        private string soDienThoaiCu = "";

        public ICommand ChinhSuaCommand { get; }
        public ICommand LuuCommand { get; }
        public ICommand HuyCommand { get; }

        public AccountViewModel()
        {
            Username = "Admin";
            HoTen = "Nguyễn Văn A";
            Email = "admin@gmail.com";
            SoDienThoai = "0912345678";
            VaiTro = "Admin";

            ChinhSuaCommand = new RelayCommand(_ => ChinhSua());
            LuuCommand = new RelayCommand(_ => Luu());
            HuyCommand = new RelayCommand(_ => Huy());
        }

        private void ChinhSua()
        {
            if (DangChinhSua) return;

            hoTenCu = HoTen;
            emailCu = Email;
            soDienThoaiCu = SoDienThoai;
            DangChinhSua = true;
        }

        private void Luu()
        {
            if (!DangChinhSua) return;

            if (string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Vui lòng nhập họ tên!");
                return;
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không hợp lệ!");
                return;
            }

            if (!Regex.IsMatch(SoDienThoai, @"^0\d{9}$"))
            {
                MessageBox.Show("Số điện thoại phải gồm 10 số và bắt đầu bằng 0!");
                return;
            }

            // Chỉ lưu trong ViewModel; chưa ghi vào SQL Server.

            if (userId == null)
            {
                DangChinhSua = false;
                MessageBox.Show("Đã lưu trong bản demo, chưa cập nhật SQL.");
                return;
            }

            try
            {
                bool thanhCong = accountService.CapNhatThongTin(
                    userId.Value, HoTen, Email, SoDienThoai);

                if (thanhCong)
                {
                    DangChinhSua = false;
                    MessageBox.Show("Cập nhật thông tin thành công!");
                }
                else
                {
                    MessageBox.Show("Không tìm thấy tài khoản để cập nhật!");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Không thể cập nhật thông tin vào SQL Server!");
            }

        }

        private void Huy()
        {
            if (!DangChinhSua) return;

            HoTen = hoTenCu;
            Email = emailCu;
            SoDienThoai = soDienThoaiCu;
            DangChinhSua = false;
        }
    }
}
