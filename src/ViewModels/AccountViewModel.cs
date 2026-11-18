using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CafeNetManager.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
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
            DangChinhSua = false;
            MessageBox.Show("Đã lưu thông tin trong bản demo. Chưa cập nhật SQL Server.");
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
