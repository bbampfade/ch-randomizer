using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace XMLUtilities
{
    class Program
    {
        static void Main(string[] args)
        {
            XslCompiledTransform xslt = new XslCompiledTransform(enableDebug :true);
            xslt.Load(@"..\..\..\CH2\XML\sort_CHDB.xsl");


            XmlDocument doc = new XmlDocument();
            doc.Load(args[0]);

            XmlReader xmlReadB = new XmlTextReader(new StringReader(doc.DocumentElement.OuterXml));

            string outputPath = args[1];
            using (var stream = File.OpenWrite(outputPath))
            {
                xslt.Transform(xmlReadB, null, stream);
            }
        }
    }
}
