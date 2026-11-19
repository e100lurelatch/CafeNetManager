
using CafeNetManager.Models;

namespace CafeNetManager.Services
{
    public static class UserSession
    {
        public static AccountProfile? CurrentUser { get; set; }
    }
}
