using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CH2.MVVM
{
    public abstract class BaseViewModel : IRaisePropertyChanged
    {
        /// <summary>
        /// Constructs an instance of this class
        /// </summary>
        protected BaseViewModel()
        {
        }

        /// <summary>
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged.Raise(this, args);
            //PropertyChanged?.Invoke(this, args); C#6
        }

        /// <summary>
        /// Allows derived classes to use <see cref="IRaisePropertyChanged.RaisePropertyChanged"/>
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// <see cref="IRaisePropertyChanged.RaisePropertyChanged"/>
        /// </summary>
        void IRaisePropertyChanged.RaisePropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }

    }
}
