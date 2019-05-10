using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace G3DViewer
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string info = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        protected bool SetValue<T>(ref T backingField, T value, [CallerMemberName]string propertyName = "")
        {
            if (object.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
