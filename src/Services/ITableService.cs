using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: Bàn + Order
    // ViewModel chỉ gọi qua interface này: MockTableService (dữ liệu giả) hoặc SqlTableService (SQL Server).
    public interface ITableService
    {
        // Sơ đồ bàn + gọi món
        List<CafeTable> GetTables();
        List<Drink> GetMenu();
        List<OrderItem> GetOrderItems(int tableId);
        void SaveOrderItems(int tableId, IEnumerable<OrderItem> items);
        void UpdateTableStatus(int tableId, TableStatus status);
        void TransferTable(int fromTableId, int toTableId);
        void ClearTable(int tableId);      // xóa hết món / hủy bàn
        void CheckoutTable(int tableId);   // đánh dấu order đã thanh toán, trả bàn về Trống

        // Quản lý bàn (thêm / sửa / xóa)
        int AddTable(CafeTable table);
        void UpdateTable(CafeTable table);
        void DeleteTable(int tableId);     // chỉ xóa được bàn đang Trống
    }
}
