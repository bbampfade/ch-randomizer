using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CH2.MVVM.Converters
{
    class RoundTextIndexConverter : ItemsControlIndexConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string idx = base.Convert(value, targetType, parameter, culture) as string;

            return String.Format("ROUND {0}", idx);
        }
    }
}
