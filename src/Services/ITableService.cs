using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo
    // ViewModel chỉ gọi qua interface này. Hiện dùng MockTableService (dữ liệu giả).
    // Khi nhóm có DataProvider + SQL, viết SqlTableService cài đặt interface này
    // rồi đổi 1 dòng trong TableViewModel là xong, không phải sửa giao diện.
    public interface ITableService
    {
        List<CafeTable> GetTables();
        List<Drink> GetMenu();
        List<OrderItem> GetOrderItems(int tableId);
        void SaveOrderItems(int tableId, IEnumerable<OrderItem> items);
        void UpdateTableStatus(int tableId, TableStatus status);
        void TransferTable(int fromTableId, int toTableId);
        void ClearTable(int tableId);      // xóa hết món / hủy bàn
        void CheckoutTable(int tableId);   // thanh toán xong, trả bàn về Trống
    }
}
