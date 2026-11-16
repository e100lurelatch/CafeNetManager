using System.Data;
using CafeNetManager.DAL;
using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: ưu đãi trên SQL Server (bảng Promotions)
    public class SqlPromotionService : IPromotionService
    {
        private readonly DataProvider _db = DataProvider.Instance;

        public List<Promotion> GetAll() =>
            _db.ExecuteQuery("SELECT * FROM Promotions ORDER BY PromotionId DESC").AsEnumerable().Select(Map).ToList();

        public Promotion? GetByCode(string code)
        {
            var dt = _db.ExecuteQuery("SELECT * FROM Promotions WHERE Code = @c", new() { ["@c"] = code.Trim() });
            return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
        }

        public int Add(Promotion p)
        {
            if (GetByCode(p.Code) != null) throw new InvalidOperationException($"Mã \"{p.Code}\" đã tồn tại.");
            return Convert.ToInt32(_db.ExecuteScalar(@"
                INSERT INTO Promotions(Code, Name, DiscountType, DiscountValue, MinOrderAmount, MaxDiscount,
                                       StartDate, EndDate, UsageLimit, UsedCount, IsActive)
                VALUES(@code, @name, @type, @value, @min, @max, @start, @end, @limit, 0, @active);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", Params(p)));
        }

        public void Update(Promotion p)
        {
            var dup = _db.ExecuteScalar("SELECT COUNT(*) FROM Promotions WHERE Code = @c AND PromotionId <> @id",
                new() { ["@c"] = p.Code, ["@id"] = p.PromotionId });
            if (Convert.ToInt32(dup) > 0) throw new InvalidOperationException($"Mã \"{p.Code}\" đã tồn tại.");

            var prm = Params(p);
            prm["@id"] = p.PromotionId;
            _db.ExecuteNonQuery(@"
                UPDATE Promotions SET Code = @code, Name = @name, DiscountType = @type, DiscountValue = @value,
                       MinOrderAmount = @min, MaxDiscount = @max, StartDate = @start, EndDate = @end,
                       UsageLimit = @limit, IsActive = @active
                WHERE PromotionId = @id", prm);
        }

        // Hóa đơn cũ vẫn giữ mã dạng chữ (PromotionCode), khóa ngoại tự về NULL
        public void Delete(int id) =>
            _db.ExecuteNonQuery("DELETE FROM Promotions WHERE PromotionId = @id", new() { ["@id"] = id });

        private static Dictionary<string, object?> Params(Promotion p) => new()
        {
            ["@code"] = p.Code, ["@name"] = p.Name, ["@type"] = (int)p.DiscountType, ["@value"] = p.DiscountValue,
            ["@min"] = p.MinOrderAmount, ["@max"] = p.MaxDiscount, ["@start"] = p.StartDate.Date,
            ["@end"] = p.EndDate.Date, ["@limit"] = p.UsageLimit, ["@active"] = p.IsActive
        };

        private static Promotion Map(DataRow r) => new()
        {
            PromotionId = r.Field<int>("PromotionId"),
            Code = r.Field<string>("Code") ?? "",
            Name = r.Field<string>("Name") ?? "",
            DiscountType = (DiscountType)r.Field<int>("DiscountType"),
            DiscountValue = r.Field<decimal>("DiscountValue"),
            MinOrderAmount = r.Field<decimal>("MinOrderAmount"),
            MaxDiscount = r.Field<decimal?>("MaxDiscount"),
            StartDate = r.Field<DateTime>("StartDate"),
            EndDate = r.Field<DateTime>("EndDate"),
            UsageLimit = r.Field<int?>("UsageLimit"),
            UsedCount = r.Field<int>("UsedCount"),
            IsActive = r.Field<bool>("IsActive")
        };
    }
}
