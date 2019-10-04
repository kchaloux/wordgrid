using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WordGrid.ViewModels
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool SetProperty<T>(ref T reference, T value, [CallerMemberName] string caller = "")
        {
            if (Equals(reference, value))
            {
                return false;
            }
            reference = value;
            OnPropertyChanged(caller);
            return true;
        }
    }
}
