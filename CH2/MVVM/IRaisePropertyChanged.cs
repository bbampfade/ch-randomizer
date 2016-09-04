using System.ComponentModel;

namespace CH2.MVVM
{
    public interface IRaisePropertyChanged : INotifyPropertyChanged
    {
        void RaisePropertyChanged(string propertyName);
    }
}
