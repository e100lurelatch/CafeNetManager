using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CafeNetManager.ViewModels
{
    // Lớp cha cho mọi ViewModel: báo cho giao diện biết khi dữ liệu thay đổi.
    // Cả nhóm có thể dùng chung lớp này.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
