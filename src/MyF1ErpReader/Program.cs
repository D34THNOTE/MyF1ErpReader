using System.Text;
using System.Xml;
using System.Xml.Linq;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;

using EgoEngineLibrary.Xml;

using EgoErpArchiver.ViewModel;

using MyF1ErpReader.Models;

namespace MyF1ErpReaderTOBEDESTROYED
{
    class Program
    {
        public static MainViewModel MainViewModel = new MainViewModel();
        
        static void Main(string[] args)
        {
            string xmlContent = GetXMLContent();

            XNamespace ns = "http://schemas.codemasters.com/compound_editor"; // necessary for reading the XML
            
            string compoundName = "2022_SUPF"; // the tyre we want to read content for
            

            XElement root = XElement.Parse(xmlContent);
            IEnumerable<XElement> temperatureNodes = root.Descendants(ns + "Compound")
                .Where(compound => (string)compound.Attribute("name") == compoundName)
                .Descendants(ns + "TemperatureGrip")
                .Descendants(ns + "SplineElement");

            CompoundInfo processCompoundInfo = ProcessCompoundInfo(compoundName, ns, root);

            // Print the extracted values
            for (int i = 0; i < processCompoundInfo.tyreTempsCelcius_Inside.Count; i++)
            {
                Console.WriteLine($"Temperature: {processCompoundInfo.tyreTempsCelcius_Inside[i]} C, Grip: {processCompoundInfo.tempsGripPercentage_Inside[i]}%");
            }
        }


        public static CompoundInfo ProcessCompoundInfo(string compoundName, XNamespace ns, XElement root)
        {
            IEnumerable<XElement> temperatureNodes = root.Descendants(ns + "Compound")
                .Where(compound => (string)compound.Attribute("name") == compoundName)
                .Descendants(ns + "TemperatureGripCarcas")
                .Descendants(ns + "SplineElement");
            
            CompoundInfo returnInfo = new CompoundInfo();

            returnInfo.compoundName = compoundName;
            
            // Adding data about tyres and formatting it
            foreach (XElement node in temperatureNodes)
            {
                double temperature = (double)node.Attribute("x") - 273.15; // C = K - 273.15 (K - Kelvin, C - Celsius)
                double grip = (double)node.Attribute("y") * 100; // no further formatting for now, but this represents the percentage
                
                returnInfo.tyreTempsCelcius_Inside.Add(temperature);
                returnInfo.tempsGripPercentage_Inside.Add(grip);
            }

            return returnInfo;
        }
        
        
        
        public static string GetXMLContent()
        {
            // Reading the .erp file
            //TODO provide a console input method
            
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