using System.Windows.Controls;

namespace CafeNetManager.Views
{
    /// <summary>
    /// Khung hiển thị tạm thời cho các module chưa được gắn trực tiếp vào MainWindow.
    /// </summary>
    public partial class DashboardPlaceholderView : UserControl
    {
        public DashboardPlaceholderView()
        {
            InitializeComponent();
        }

        public DashboardPlaceholderView(
            string moduleName,
            string iconGlyph = "\uE80F",
            string? description = null) : this()
        {
            txtModuleName.Text = moduleName;
            txtIcon.Text = iconGlyph;

            if (!string.IsNullOrWhiteSpace(description))
            {
                txtDescription.Text = description;
            }
        }
    }
}
