
using System.Data;
using CafeNetManager.DAL;
using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    public class AccountService
    {

        public AccountProfile? XacThucDangNhap(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password))
                return null;

            string sql = @"
        SELECT PasswordHash
        FROM dbo.Users
        WHERE Username = @username AND IsActive = 1";

            var parameters = new Dictionary<string, object?>
            {
                ["@username"] = username
            };

            DataTable dt = DataProvider.Instance.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count == 0)
                return null;

            string hash = dt.Rows[0].Field<string>("PasswordHash") ?? "";

            if (!PasswordHasher.KiemTraMatKhau(password, hash))
                return null;

            return LayTaiKhoan(username);
        }

        public bool CapNhatThongTin(int userId, string hoTen,
                                    string email, string soDienThoai)
        {
            string sql = @"
        UPDATE dbo.Users
        SET FullName = @hoTen,
            Email = @email,
            Phone = @soDienThoai
        WHERE UserId = @userId AND IsActive = 1";

            var parameters = new Dictionary<string, object?>
            {
                ["@userId"] = userId,
                ["@hoTen"] = hoTen,
                ["@email"] = email,
                ["@soDienThoai"] = soDienThoai
            };

            int soDong = DataProvider.Instance.ExecuteNonQuery(sql, parameters);

            return soDong == 1;
        }

        public AccountProfile? LayTaiKhoan(string username)
        {
            string sql = @"
                SELECT UserId, Username, FullName,
                       Email, Phone, Role, IsActive
                FROM dbo.Users
                WHERE Username = @username";

            var parameters = new Dictionary<string, object?>
            {
                ["@username"] = username
            };

            DataTable dt = DataProvider.Instance.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count == 0)
                return null;

            DataRow r = dt.Rows[0];

            return new AccountProfile
            {
                UserId = r.Field<int>("UserId"),
                Username = r.Field<string>("Username") ?? "",
                FullName = r.Field<string>("FullName") ?? "",
                Email = r.Field<string>("Email") ?? "",
                Phone = r.Field<string>("Phone") ?? "",
                Role = r.Field<string>("Role") ?? "",
                IsActive = r.Field<bool>("IsActive")
            };
        }

        public bool DoiMatKhau(int userId, string matKhauCu, string matKhauMoi)
        {
            if (string.IsNullOrEmpty(matKhauCu) ||
                string.IsNullOrEmpty(matKhauMoi) ||
                matKhauMoi.Length < 8)
                return false;

            string sql = @"
        SELECT PasswordHash
        FROM dbo.Users
        WHERE UserId = @userId AND IsActive = 1";

            var parameters = new Dictionary<string, object?>
            {
                ["@userId"] = userId
            };

            DataTable dt = DataProvider.Instance.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count == 0)
                return false;

            string hashCu = dt.Rows[0].Field<string>("PasswordHash") ?? "";

            if (!PasswordHasher.KiemTraMatKhau(matKhauCu, hashCu))
                return false;

            string hashMoi = PasswordHasher.BamMatKhau(matKhauMoi);

            string updateSql = @"
        UPDATE dbo.Users
        SET PasswordHash = @hashMoi
        WHERE UserId = @userId
          AND IsActive = 1
          AND PasswordHash = @hashCu";

            var updateParameters = new Dictionary<string, object?>
            {
                ["@userId"] = userId,
                ["@hashMoi"] = hashMoi,
                ["@hashCu"] = hashCu
            };

            int soDong = DataProvider.Instance.ExecuteNonQuery(
                updateSql, updateParameters);

            return soDong == 1;
        }

    }
}
