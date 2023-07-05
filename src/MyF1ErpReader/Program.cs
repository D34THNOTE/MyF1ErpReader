using System.Text;
using System.Xml;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;

using EgoEngineLibrary.Xml;

using EgoErpArchiver.ViewModel;

namespace MyF1ErpReaderTOBEDESTROYED
{
    class Program
    {
        public static MainViewModel MainViewModel = new MainViewModel();
        
        static void Main(string[] args)
        {
            string xmlContent = GetXMLContent();

            List<double> temperatures = new List<double>();
            List<double> grips = new List<double>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "http://schemas.codemasters.com/compound_editor");

            XmlNodeList temperatureNodes = xmlDoc.SelectNodes("//ns:Temperature/ns:TemperatureGrip/ns:SplineElement", nsManager);

            foreach (XmlNode node in temperatureNodes)
            {
                double temperature = Convert.ToDouble(node.Attributes["x"].Value);
                double grip = Convert.ToDouble(node.Attributes["y"].Value);
                temperatures.Add(temperature);
                grips.Add(grip);
            }

            // printing out all temperatures
            foreach (var line in temperatures)
            {
                Console.WriteLine(line);
            }
        }


        public static string GetXMLContent()
        {
            // Reading the .erp file
            MainViewModel.Open("C:\\Users\\borsu\\OneDrive\\Pulpit\\My coding stuff\\C#\\MyF1ErpReader\\f1-22-tyrecompounds.erp");

            // obtaining the first element, it is written in such a way that it can theoretically store multiple XML files but we only load and want the first(and only) one
            var readXmlFile = MainViewModel.XmlFilesWorkspace.XmlFiles[0];

            ErpFragment fragment = readXmlFile.Fragment;

            byte[] data = fragment.GetDataArray(decompress: true);
            string xmlContent = Encoding.UTF8.GetString(data);

            return xmlContent;
        }
    }
}