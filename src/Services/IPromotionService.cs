using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    // Module 02 - Bảo: Quản lý ưu đãi
    public interface IPromotionService
    {
        List<Promotion> GetAll();
        Promotion? GetByCode(string code);
        int Add(Promotion promotion);
        void Update(Promotion promotion);
        void Delete(int promotionId);
    }
}
