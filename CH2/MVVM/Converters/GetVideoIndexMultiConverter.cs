using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace CH2.MVVM.Converters
{
    class GetVideoIndexMultiConverter: GetIndexMultiConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var itemIndex = base.Convert(values, targetType, parameter, culture);

            var video = (XElement)values[0];

            var filename = Path.GetFileNameWithoutExtension(video.Element("Filename").Value);

            var retVal = string.Format("{0,2} = {1,-68} online", itemIndex, filename);

            Console.WriteLine("Length: {0}", retVal.Length);

            return retVal;
        }

    }
}
