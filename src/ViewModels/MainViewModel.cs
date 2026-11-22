using System;
using System.Windows.Input;
using CafeNetManager.Views;

namespace CafeNetManager.ViewModels
{
    /// <summary>
    /// Dữ liệu hiển thị cho trang mặc định "Tổng quan" (render qua DataTemplate trong MainWindow.xaml).
    /// </summary>
    public sealed class DashboardHomeContent
    {
        public string UserName { get; }
        public string CurrentDateText { get; }

        public DashboardHomeContent(string userName, string currentDateText)
        {
            UserName = string.IsNullOrWhiteSpace(userName) ? "Admin" : userName;
            CurrentDateText = currentDateText;
        }
    }

    /// <summary>
    /// ViewModel điều hướng chính cho MainWindow của ÊM COFFEE - CafeNetManager.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private object? _currentView;
        private string _currentTitle = string.Empty;
        private string _currentSubtitle = string.Empty;
        private string _currentUserName;
        private string _currentUserRole;

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string CurrentTitle
        {
            get => _currentTitle;
            set => SetProperty(ref _currentTitle, value);
        }

        public string CurrentSubtitle
        {
            get => _currentSubtitle;
            set => SetProperty(ref _currentSubtitle, value);
        }

        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                if (SetProperty(ref _currentUserName, value))
                {
                    OnPropertyChanged(nameof(CurrentUserInitial));
                }
            }
        }

        public string CurrentUserRole
        {
            get => _currentUserRole;
            set => SetProperty(ref _currentUserRole, value);
        }

        public string CurrentUserInitial =>
            string.IsNullOrWhiteSpace(CurrentUserName)
                ? "A"
                : CurrentUserName.Trim()[0].ToString().ToUpperInvariant();

        public string CurrentDateText => DateTime.Now.ToString("dd/MM/yyyy");

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        public event Action? LogoutRequested;

        public MainViewModel()
        {
            // Services/UserSession.cs chưa có trên nhánh này nên dùng giá trị mặc định.
            // Sau khi nhóm merge Services/UserSession.cs, có thể đọc trực tiếp:
            // _currentUserName = Services.UserSession.CurrentUser?.Username ?? "Admin";
            // _currentUserRole = string.IsNullOrWhiteSpace(Services.UserSession.CurrentUser?.Role)
            //     ? "Quản trị viên"
            //     : Services.UserSession.CurrentUser.Role;
            _currentUserName = "Admin";
            _currentUserRole = "Quản trị viên";

            NavigateCommand = new RelayCommand(param => Navigate(param?.ToString() ?? "Dashboard"));
            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke());

            // Mặc định mở mục "Tổng quan" khi khởi tạo MainWindow
            Navigate("Dashboard");
        }

        private void Navigate(string menuKey)
        {
            switch (menuKey)
            {
                case "Dashboard":
                    CurrentTitle = "Tổng quan";
                    CurrentSubtitle = "Chào mừng quay trở lại hệ thống quản lý ÊM COFFEE";
                    CurrentView = new DashboardHomeContent(CurrentUserName, CurrentDateText);
                    break;

                case "Table":
                    CurrentTitle = "Sơ đồ bàn & Gọi món";
                    CurrentSubtitle = "Theo dõi trạng thái bàn, tạo đơn gọi món và thanh toán";
                    // TODO: Thay Placeholder bằng View thật: CurrentView = new TableView();
                    CurrentView = new DashboardPlaceholderView(
                        "Sơ đồ bàn & Gọi món",
                        "\uE80A",
                        "Quản lý sơ đồ bàn, thêm đồ uống vào hóa đơn và xử lý thanh toán.");
                    break;

                case "TableManager":
                    CurrentTitle = "Quản lý bàn";
                    CurrentSubtitle = "Thêm, sửa, xóa và cấu hình danh sách bàn trong quán";
                    // TODO: Thay Placeholder bằng View thật: CurrentView = new TableManagerView();
                    CurrentView = new DashboardPlaceholderView(
                        "Quản lý bàn",
                        "\uE713",
                        "Cấu hình khu vực, số lượng và trạng thái bàn của quán.");
                    break;

                case "Promotion":
                    CurrentTitle = "Ưu đãi";
                    CurrentSubtitle = "Quản lý mã giảm giá và chương trình khuyến mãi";
                    // TODO: Thay Placeholder bằng View thật: CurrentView = new PromotionView();
                    CurrentView = new DashboardPlaceholderView(
                        "Ưu đãi",
                        "\uE8EC",
                        "Thiết lập chương trình ưu đãi, voucher và mức giảm giá áp dụng.");
                    break;

                case "InvoiceHistory":
                    CurrentTitle = "Lịch sử hóa đơn";
                    CurrentSubtitle = "Tra cứu, lọc và in lại các hóa đơn đã thanh toán";
                    // TODO: Thay Placeholder bằng View thật: CurrentView = new InvoiceHistoryView();
                    CurrentView = new DashboardPlaceholderView(
                        "Lịch sử hóa đơn",
                        "\uE81C",
                        "Xem danh sách hóa đơn đã xuất, chi tiết món và tổng doanh thu.");
                    break;

                case "Account":
                    CurrentTitle = "Thông tin tài khoản";
                    CurrentSubtitle = "Quản lý hồ sơ người dùng và bảo mật tài khoản";
                    // TODO: Thay Placeholder bằng View thật: CurrentView = new AccountView();
                    CurrentView = new DashboardPlaceholderView(
                        "Thông tin tài khoản",
                        "\uE77B",
                        "Cập nhật thông tin cá nhân, vai trò và đổi mật khẩu đăng nhập.");
                    break;

                default:
                    CurrentTitle = "Tổng quan";
                    CurrentSubtitle = "Chào mừng quay trở lại hệ thống quản lý ÊM COFFEE";
                    CurrentView = new DashboardHomeContent(CurrentUserName, CurrentDateText);
                    break;
            }
        }
    }
}
