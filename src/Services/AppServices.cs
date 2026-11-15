using System.ComponentModel;
using System.Windows;
using CafeNetManager.DAL;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: nơi tạo các service dùng chung cho mọi màn hình.
    // Kết nối được SQL Server -> dùng Sql...Service; không thì dùng Mock...Service (dữ liệu giả).
    public static class AppServices
    {
        public static bool UseSql { get; }
        public static ITableService Tables { get; }
        public static IPromotionService Promotions { get; }
        public static IInvoiceService Invoices { get; }

        // Báo cho các màn hình khác tải lại dữ liệu
        public static event Action? TablesChanged;
        public static event Action? InvoicesChanged;
        public static void NotifyTablesChanged() => TablesChanged?.Invoke();
        public static void NotifyInvoicesChanged() => InvoicesChanged?.Invoke();

        static AppServices()
        {
            UseSql = !IsDesignMode() && CanConnectSql();

            if (UseSql)
            {
                Tables = new SqlTableService();
                Promotions = new SqlPromotionService();
                Invoices = new SqlInvoiceService();
            }
            else
            {
                var tables = new MockTableService();
                var promotions = new MockPromotionService();
                Tables = tables;
                Promotions = promotions;
                Invoices = new MockInvoiceService(tables, promotions);
            }
        }

        private static bool IsDesignMode() =>
            DesignerProperties.GetIsInDesignMode(new DependencyObject());

        private static bool CanConnectSql()
        {
            try
            {
                var error = DataProvider.Instance.TestConnection();
                if (error == null) return true;
                MessageBox.Show("Không kết nối được SQL Server, đang dùng dữ liệu giả.\n\n"
                              + "Kiểm tra: đã chạy database/CafeNetManager.sql chưa, Server= trong App.config đúng chưa.\n\n"
                              + "Chi tiết: " + error, "Kết nối CSDL", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cấu hình CSDL: " + ex.Message, "Kết nối CSDL",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return false;
        }
    }
}
