using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace CH2.MVVM.Converters
{
    public class GetIndexMultiConverter : IMultiValueConverter
    {
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IList)values[1]; // have to cast to interface because this might be a templated type 
            var itemIndex = collection.IndexOf(values[0]);

            return itemIndex;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("GetIndexMultiConverter_ConvertBack");
        }
    }
}
