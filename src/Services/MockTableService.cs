using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: dữ liệu giả để làm giao diện khi chưa có SQL.
    // Dữ liệu chỉ nằm trong RAM, tắt app là mất.
    public class MockTableService : ITableService
    {
        private readonly List<CafeTable> _tables;
        private readonly List<Drink> _menu;
        private readonly Dictionary<int, List<OrderItem>> _orders = new();

        public MockTableService()
        {
            _tables = Enumerable.Range(1, 12).Select(i => new CafeTable
            {
                TableId = i,
                Name = $"Bàn {i}",
                Capacity = i <= 2 ? 2 : (i >= 11 ? 6 : 4),
                Status = TableStatus.Empty
            }).ToList();

            _menu = new List<Drink>
            {
                new() { DrinkId = 1, Name = "Cà phê đen",   Category = "Cà phê",  Price = 20000 },
                new() { DrinkId = 2, Name = "Cà phê sữa",   Category = "Cà phê",  Price = 25000 },
                new() { DrinkId = 3, Name = "Bạc xỉu",      Category = "Cà phê",  Price = 28000 },
                new() { DrinkId = 4, Name = "Trà đào",      Category = "Trà",     Price = 30000 },
                new() { DrinkId = 5, Name = "Trà vải",      Category = "Trà",     Price = 30000 },
                new() { DrinkId = 6, Name = "Sinh tố bơ",   Category = "Sinh tố", Price = 35000 },
                new() { DrinkId = 7, Name = "Bánh flan",    Category = "Bánh",    Price = 15000 },
            };

            // Vài bàn có sẵn trạng thái để thấy màu
            _tables[2].Status = TableStatus.Occupied;
            _orders[3] = new List<OrderItem>
            {
                new() { DrinkId = 2, DrinkName = "Cà phê sữa", UnitPrice = 25000, Quantity = 2, Note = "Ít đường" }
            };
            _tables[6].Status = TableStatus.Reserved;
        }

        public List<CafeTable> GetTables() => _tables;
        public List<Drink> GetMenu() => _menu;

        public List<OrderItem> GetOrderItems(int tableId) =>
            _orders.TryGetValue(tableId, out var items)
                ? items.Select(Clone).ToList()
                : new List<OrderItem>();

        public void SaveOrderItems(int tableId, IEnumerable<OrderItem> items) =>
            _orders[tableId] = items.Select(Clone).ToList();

        public void UpdateTableStatus(int tableId, TableStatus status) =>
            _tables.First(t => t.TableId == tableId).Status = status;

        public void TransferTable(int fromTableId, int toTableId)
        {
            if (_orders.Remove(fromTableId, out var items))
                _orders[toTableId] = items;
            UpdateTableStatus(toTableId, TableStatus.Occupied);
            UpdateTableStatus(fromTableId, TableStatus.Empty);
        }

        public void ClearTable(int tableId)
        {
            _orders.Remove(tableId);
            UpdateTableStatus(tableId, TableStatus.Empty);
        }

        public void CheckoutTable(int tableId) => ClearTable(tableId);

        // ===== Quản lý bàn =====
        public int AddTable(CafeTable table)
        {
            if (_tables.Any(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Đã có bàn tên \"{table.Name}\".");
            table.TableId = _tables.Count == 0 ? 1 : _tables.Max(t => t.TableId) + 1;
            table.Status = TableStatus.Empty;
            _tables.Add(table);
            return table.TableId;
        }

        public void UpdateTable(CafeTable table)
        {
            if (_tables.Any(t => t.TableId != table.TableId && t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Đã có bàn tên \"{table.Name}\".");
            var t = _tables.First(x => x.TableId == table.TableId);
            t.Name = table.Name;
            t.Capacity = table.Capacity;
        }

        public void DeleteTable(int tableId)
        {
            var t = _tables.First(x => x.TableId == tableId);
            if (t.Status != TableStatus.Empty)
                throw new InvalidOperationException($"{t.Name} đang có khách hoặc đã đặt trước, không xóa được.");
            _tables.Remove(t);
            _orders.Remove(tableId);
        }

        private static OrderItem Clone(OrderItem i) => new()
        {
            DrinkId = i.DrinkId, DrinkName = i.DrinkName, UnitPrice = i.UnitPrice,
            Quantity = i.Quantity, Note = i.Note
        };
    }
}
