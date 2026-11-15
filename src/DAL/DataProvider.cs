using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CafeNetManager.DAL
{
    // Lớp dùng chung cho cả nhóm để gọi SQL Server.
    // Chuỗi kết nối lấy từ App.config (name="CafeDB").
    // Luôn truyền tham số dạng @ten, không cộng chuỗi SQL (tránh SQL Injection).
    public sealed class DataProvider
    {
        private static DataProvider? _instance;
        public static DataProvider Instance => _instance ??= new DataProvider();

        public string ConnectionString { get; }

        private DataProvider()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["CafeDB"]?.ConnectionString
                ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối 'CafeDB' trong App.config");
        }

        private static SqlCommand CreateCommand(SqlConnection conn, string sql,
                                                Dictionary<string, object?>? parameters,
                                                SqlTransaction? tran = null)
        {
            var cmd = new SqlCommand(sql, conn, tran);
            if (parameters != null)
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
            return cmd;
        }

        /// <summary>SELECT -> trả về DataTable</summary>
        public DataTable ExecuteQuery(string sql, Dictionary<string, object?>? parameters = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = CreateCommand(conn, sql, parameters);
            using var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        /// <summary>INSERT / UPDATE / DELETE / EXEC -> số dòng bị ảnh hưởng</summary>
        public int ExecuteNonQuery(string sql, Dictionary<string, object?>? parameters = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var cmd = CreateCommand(conn, sql, parameters);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>Lấy 1 giá trị (COUNT, SCOPE_IDENTITY...)</summary>
        public object? ExecuteScalar(string sql, Dictionary<string, object?>? parameters = null)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var cmd = CreateCommand(conn, sql, parameters);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? null : result;
        }

        /// <summary>Chạy nhiều lệnh trong 1 transaction: lỗi giữa chừng thì hủy hết.</summary>
        public void ExecuteInTransaction(Action<SqlConnection, SqlTransaction> work)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();
            try { work(conn, tran); tran.Commit(); }
            catch { tran.Rollback(); throw; }
        }

        public static SqlCommand Command(SqlConnection conn, SqlTransaction tran, string sql,
                                         Dictionary<string, object?>? parameters = null)
            => CreateCommand(conn, sql, parameters, tran);

        /// <summary>Kiểm tra kết nối. Trả về null nếu OK, ngược lại trả về thông báo lỗi.</summary>
        public string? TestConnection()
        {
            try
            {
                using var conn = new SqlConnection(ConnectionString);
                conn.Open();
                return null;
            }
            catch (Exception ex) { return ex.Message; }
        }
    }
}
