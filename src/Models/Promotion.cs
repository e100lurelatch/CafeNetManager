namespace CafeNetManager.Models
{
    // Module 02 - Bảo: Ưu đãi / mã giảm giá
    public enum DiscountType
    {
        Percent = 0,   // giảm theo %
        Amount = 1     // giảm số tiền cố định
    }

    public class Promotion
    {
        public int PromotionId { get; set; }
        public string Code { get; set; } = string.Empty;        // VD: GIAM10
        public string Name { get; set; } = string.Empty;        // VD: Giảm 10% cuối tuần
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }               // 10 (%) hoặc 20000 (đ)
        public decimal MinOrderAmount { get; set; }              // đơn tối thiểu
        public decimal? MaxDiscount { get; set; }                // giảm tối đa (chỉ cho loại %)
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);
        public int? UsageLimit { get; set; }                     // null = không giới hạn
        public int UsedCount { get; set; }
        public bool IsActive { get; set; } = true;

        // ===== Chuỗi hiển thị =====
        public string DiscountText => DiscountType == DiscountType.Percent
            ? $"Giảm {DiscountValue:0.##}%" + (MaxDiscount > 0 ? $" (tối đa {MaxDiscount:N0}đ)" : "")
            : $"Giảm {DiscountValue:N0}đ";

        public string ConditionText => MinOrderAmount > 0 ? $"Đơn từ {MinOrderAmount:N0}đ" : "Mọi đơn";
        public string PeriodText => $"{StartDate:dd/MM/yyyy} - {EndDate:dd/MM/yyyy}";
        public string UsageText => UsageLimit.HasValue ? $"{UsedCount}/{UsageLimit}" : $"{UsedCount}/∞";
        public string DisplayText => $"{Code} - {DiscountText}";

        public string StatusText
        {
            get
            {
                var today = DateTime.Today;
                if (!IsActive) return "Đã tắt";
                if (today < StartDate.Date) return "Chưa bắt đầu";
                if (today > EndDate.Date) return "Hết hạn";
                if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value) return "Hết lượt";
                return "Đang chạy";
            }
        }

        /// <summary>Kiểm tra mã có dùng được cho đơn này không. Trả về null nếu dùng được, ngược lại là lý do.</summary>
        public string? CheckApplicable(decimal subTotal, DateTime now)
        {
            if (!IsActive) return "Mã ưu đãi đã bị tắt.";
            if (now.Date < StartDate.Date) return $"Mã chỉ áp dụng từ ngày {StartDate:dd/MM/yyyy}.";
            if (now.Date > EndDate.Date) return $"Mã đã hết hạn từ ngày {EndDate:dd/MM/yyyy}.";
            if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value) return "Mã đã hết lượt sử dụng.";
            if (subTotal < MinOrderAmount) return $"Đơn hàng phải từ {MinOrderAmount:N0}đ mới dùng được mã này.";
            return null;
        }

        /// <summary>Số tiền được giảm (đã làm tròn, không vượt quá tiền đơn).</summary>
        public decimal CalculateDiscount(decimal subTotal)
        {
            decimal discount = DiscountType == DiscountType.Percent
                ? Math.Round(subTotal * DiscountValue / 100m, 0)
                : DiscountValue;

            if (DiscountType == DiscountType.Percent && MaxDiscount > 0)
                discount = Math.Min(discount, MaxDiscount.Value);

            return Math.Max(0, Math.Min(discount, subTotal));
        }
    }
}
