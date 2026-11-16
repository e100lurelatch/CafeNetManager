using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: ưu đãi dữ liệu giả (chỉ nằm trong RAM)
    public class MockPromotionService : IPromotionService
    {
        private readonly List<Promotion> _items = new()
        {
            new() { PromotionId = 1, Code = "GIAM10", Name = "Giảm 10% mọi đơn", DiscountType = DiscountType.Percent,
                    DiscountValue = 10, MaxDiscount = 30000, StartDate = DateTime.Today.AddDays(-7), EndDate = DateTime.Today.AddMonths(2) },
            new() { PromotionId = 2, Code = "KHAITRUONG", Name = "Khai trương giảm 20.000đ", DiscountType = DiscountType.Amount,
                    DiscountValue = 20000, MinOrderAmount = 100000, StartDate = DateTime.Today.AddDays(-1),
                    EndDate = DateTime.Today.AddDays(30), UsageLimit = 50 },
            new() { PromotionId = 3, Code = "HSSV15", Name = "Học sinh sinh viên giảm 15%", DiscountType = DiscountType.Percent,
                    DiscountValue = 15, MinOrderAmount = 50000, MaxDiscount = 25000,
                    StartDate = DateTime.Today.AddMonths(-1), EndDate = DateTime.Today.AddMonths(6) },
            new() { PromotionId = 4, Code = "TET2026", Name = "Ưu đãi Tết (đã hết hạn)", DiscountType = DiscountType.Percent,
                    DiscountValue = 20, StartDate = new DateTime(2026, 1, 20), EndDate = new DateTime(2026, 2, 20) },
        };

        public List<Promotion> GetAll() => _items.OrderByDescending(p => p.PromotionId).ToList();

        public Promotion? GetByCode(string code) =>
            _items.FirstOrDefault(p => p.Code.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase));

        public int Add(Promotion p)
        {
            if (GetByCode(p.Code) != null) throw new InvalidOperationException($"Mã \"{p.Code}\" đã tồn tại.");
            p.PromotionId = _items.Count == 0 ? 1 : _items.Max(x => x.PromotionId) + 1;
            _items.Add(p);
            return p.PromotionId;
        }

        public void Update(Promotion p)
        {
            if (_items.Any(x => x.PromotionId != p.PromotionId && x.Code.Equals(p.Code, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Mã \"{p.Code}\" đã tồn tại.");
            var i = _items.FindIndex(x => x.PromotionId == p.PromotionId);
            p.UsedCount = _items[i].UsedCount;   // lượt đã dùng không sửa tay
            _items[i] = p;
        }

        public void Delete(int id) => _items.RemoveAll(p => p.PromotionId == id);

        /// <summary>Gọi khi thanh toán có dùng mã.</summary>
        public void IncreaseUsage(int id)
        {
            var p = _items.First(x => x.PromotionId == id);
            if (p.UsageLimit.HasValue && p.UsedCount >= p.UsageLimit.Value)
                throw new InvalidOperationException("Mã ưu đãi đã hết lượt sử dụng.");
            p.UsedCount++;
        }
    }
}
