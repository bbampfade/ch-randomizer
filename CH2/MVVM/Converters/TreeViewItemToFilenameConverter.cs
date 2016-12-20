using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;

namespace CH2.MVVM.Converters
{
    class TreeViewItemToFilenameConverter : IValueConverter
    {
        private XName videoName = XName.Get("VIDEO");
        private XName roundName = XName.Get("Round");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (typeof(XElement).IsAssignableFrom(value.GetType()))
                {
                    var elem = value as XElement;
                    if (elem.Name.Equals(videoName))
                    {
                        return elem.Element("Filename").Value;
                    }
                    else // must be a round
                    {
                        return elem.Parent.Element("Filename").Value;
                    }
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
