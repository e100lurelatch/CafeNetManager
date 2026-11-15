namespace CafeNetManager.Models
{
    // Món trong menu. Quân (Module 01) quản lý CRUD menu,
    // Bảo chỉ đọc danh sách này để gọi món. Nếu Quân đặt tên khác (VD: DoUong) thì thống nhất lại.
    public class Drink
    {
        public int DrinkId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
