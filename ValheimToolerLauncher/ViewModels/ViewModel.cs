using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ValheimToolerLauncher.ViewModels
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T property, T value, [CallerMemberName] string name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(property, value))
            {
                property = value;
                RaisePropertyChanged(name);
            }
        }

        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
