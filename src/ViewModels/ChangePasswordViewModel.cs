namespace CafeNetManager.ViewModels
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        // Chỉ kiểm tra dữ liệu nhập. Không xác thực mật khẩu cũ hoặc ghi SQL.
        public string KiemTraMatKhau(string matKhauCu, string matKhauMoi, string xacNhan)
        {
            if (string.IsNullOrWhiteSpace(matKhauCu) ||
                string.IsNullOrWhiteSpace(matKhauMoi) ||
                string.IsNullOrWhiteSpace(xacNhan))
                return "Vui lòng nhập đầy đủ thông tin!";

            if (matKhauMoi.Length < 8)
                return "Mật khẩu mới phải có ít nhất 8 ký tự!";

            if (matKhauMoi != xacNhan)
                return "Mật khẩu xác nhận không khớp!";

            if (matKhauMoi == matKhauCu)
                return "Mật khẩu mới phải khác mật khẩu cũ!";

            return "";
        }
    }
}
