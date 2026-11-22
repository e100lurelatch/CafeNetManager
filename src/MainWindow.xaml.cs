using System;
using System.Windows;
using CafeNetManager.ViewModels;
using CafeNetManager.Views;

namespace CafeNetManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _viewModel.LogoutRequested += OnLogoutRequested;
            DataContext = _viewModel;
        }

        private void OnLogoutRequested()
        {
            // Mở LoginView trước khi đóng MainWindow để tránh ứng dụng tự tắt (OnLastWindowClose)
            var loginView = new LoginView();
            Application.Current.MainWindow = loginView;
            loginView.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.LogoutRequested -= OnLogoutRequested;
            base.OnClosed(e);
        }
    }
}
