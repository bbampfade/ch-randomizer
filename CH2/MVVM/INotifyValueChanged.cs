using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH2.MVVM
{
    public interface INotifyValueChanged
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
}
