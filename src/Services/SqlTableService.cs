using System.Data;
using CafeNetManager.DAL;
using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: đọc/ghi Bàn + Order từ SQL Server
    // Cần chạy database/CafeNetManager.sql trước.
    public class SqlTableService : ITableService
    {
        private readonly DataProvider _db = DataProvider.Instance;

        public List<CafeTable> GetTables()
        {
            var dt = _db.ExecuteQuery("SELECT TableId, Name, Capacity, Status FROM CafeTables ORDER BY TableId");
            return dt.AsEnumerable().Select(r => new CafeTable
            {
                TableId = r.Field<int>("TableId"),
                Name = r.Field<string>("Name") ?? "",
                Capacity = r.Field<int>("Capacity"),
                Status = (TableStatus)r.Field<int>("Status")
            }).ToList();
        }

        public List<Drink> GetMenu()
        {
            var dt = _db.ExecuteQuery("SELECT DrinkId, Name, Category, Price FROM Drinks WHERE IsAvailable = 1 ORDER BY Category, Name");
            return dt.AsEnumerable().Select(r => new Drink
            {
                DrinkId = r.Field<int>("DrinkId"),
                Name = r.Field<string>("Name") ?? "",
                Category = r.Field<string>("Category") ?? "",
                Price = r.Field<decimal>("Price")
            }).ToList();
        }

        public List<OrderItem> GetOrderItems(int tableId)
        {
            var dt = _db.ExecuteQuery(@"
                SELECT od.DrinkId, d.Name AS DrinkName, od.UnitPrice, od.Quantity, od.Note
                FROM OrderDetails od
                JOIN Orders o ON o.OrderId = od.OrderId
                JOIN Drinks d ON d.DrinkId = od.DrinkId
                WHERE o.TableId = @tableId AND o.Status = 0
                ORDER BY od.OrderDetailId",
                new() { ["@tableId"] = tableId });

            return dt.AsEnumerable().Select(r => new OrderItem
            {
                DrinkId = r.Field<int>("DrinkId"),
                DrinkName = r.Field<string>("DrinkName") ?? "",
                UnitPrice = r.Field<decimal>("UnitPrice"),
                Quantity = r.Field<int>("Quantity"),
                Note = r.Field<string>("Note") ?? ""
            }).ToList();
        }

        // Lưu toàn bộ món của bàn: lấy (hoặc tạo) order chưa thanh toán, xóa chi tiết cũ, ghi lại chi tiết mới
        public void SaveOrderItems(int tableId, IEnumerable<OrderItem> items)
        {
            var list = items.ToList();
            _db.ExecuteInTransaction((conn, tran) =>
            {
                var found = DataProvider.Command(conn, tran,
                    "SELECT TOP 1 OrderId FROM Orders WHERE TableId = @t AND Status = 0",
                    new() { ["@t"] = tableId }).ExecuteScalar();

                int orderId;
                if (found == null || found == DBNull.Value)
                {
                    orderId = Convert.ToInt32(DataProvider.Command(conn, tran,
                        "INSERT INTO Orders(TableId) VALUES(@t); SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new() { ["@t"] = tableId }).ExecuteScalar());
                }
                else orderId = Convert.ToInt32(found);

                DataProvider.Command(conn, tran, "DELETE FROM OrderDetails WHERE OrderId = @o",
                    new() { ["@o"] = orderId }).ExecuteNonQuery();

                foreach (var i in list)
                {
                    DataProvider.Command(conn, tran, @"
                        INSERT INTO OrderDetails(OrderId, DrinkId, Quantity, UnitPrice, Note)
                        VALUES(@o, @d, @q, @p, @n)",
                        new() { ["@o"] = orderId, ["@d"] = i.DrinkId, ["@q"] = i.Quantity,
                                ["@p"] = i.UnitPrice, ["@n"] = i.Note ?? "" }).ExecuteNonQuery();
                }
            });
        }

        public void UpdateTableStatus(int tableId, TableStatus status) =>
            _db.ExecuteNonQuery("UPDATE CafeTables SET Status = @s WHERE TableId = @t",
                new() { ["@s"] = (int)status, ["@t"] = tableId });

        public void TransferTable(int fromTableId, int toTableId) =>
            _db.ExecuteNonQuery("EXEC sp_TransferTable @from, @to",
                new() { ["@from"] = fromTableId, ["@to"] = toTableId });

        // Xóa hết món (khách hủy): xóa order chưa thanh toán, trả bàn về Trống
        public void ClearTable(int tableId) =>
            _db.ExecuteNonQuery(@"
                DELETE FROM Orders WHERE TableId = @t AND Status = 0;
                UPDATE CafeTables SET Status = 0 WHERE TableId = @t;",
                new() { ["@t"] = tableId });

        // Thanh toán: giữ lại order (đánh dấu Đã thanh toán) để làm doanh thu, trả bàn về Trống
        public void CheckoutTable(int tableId) =>
            _db.ExecuteNonQuery(@"
                UPDATE Orders SET Status = 1 WHERE TableId = @t AND Status = 0;
                UPDATE CafeTables SET Status = 0 WHERE TableId = @t;",
                new() { ["@t"] = tableId });
    }
}
