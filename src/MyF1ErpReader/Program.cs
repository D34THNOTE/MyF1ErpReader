using System.Text;
using System.Xml;
using System.Xml.Linq;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;

using EgoEngineLibrary.Xml;

using EgoErpArchiver.ViewModel;

using MyF1ErpReader;
using MyF1ErpReader.Models;

using OfficeOpenXml;

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
            
            IEnumerable<string> tyreNames = root.Descendants(ns + "Compound")
                .Select(compound => (string)compound.Attribute("name"));

            // List of all tyre names from .erp file
            List<string> tyreNamesList = tyreNames.ToList();
            // Tyres have names like USOF and USOR, where the last letter means Front or Rear. We only need one for this application, I chose to use F variant
            tyreNamesList.RemoveAll(name => name.EndsWith("R"));

            List<CompoundInfo> tyresDetailsList = new List<CompoundInfo>();
            foreach (var tyreName in tyreNamesList)
            {
                tyresDetailsList.Add(ProcessCompoundInfo(tyreName, ns, root));
            }
            
            ConvertCompoundNames(tyresDetailsList);
            
            ExcelOutput.GenerateExcelOutput(tyresDetailsList);
        }


        public static CompoundInfo ProcessCompoundInfo(string compoundName, XNamespace ns, XElement root)
        {
            CompoundInfo returnInfo = new CompoundInfo();
            
            returnInfo.compoundName = compoundName;

            // Inside tyre temperature details
            IEnumerable<XElement> tyreDetailsXML = root.Descendants(ns + "Compound")
                .Where(compound => (string)compound.Attribute("name") == compoundName)
                .Descendants(ns + "TemperatureGripCarcas")
                .Descendants(ns + "SplineElement");
            
            foreach (XElement node in tyreDetailsXML)
            {
                double temperature = (double)node.Attribute("x") - 273.15; // C = K - 273.15 (K - Kelvin, C - Celsius)
                double grip = (double)node.Attribute("y") * 100; // no further formatting for now, but this represents the percentage
                
                returnInfo.tyreTempsCelcius_Inside.Add(temperature);
                returnInfo.tempsGripPercentage_Inside.Add(grip);
            }
            
            
            
            
            // Outside tyre temperature details
            tyreDetailsXML = root.Descendants(ns + "Compound")
                .Where(compound => (string)compound.Attribute("name") == compoundName)
                .Descendants(ns + "TemperatureGrip")
                .Descendants(ns + "SplineElement");
            
            foreach (XElement node in tyreDetailsXML)
            {
                double temperature = (double)node.Attribute("x") - 273.15; // C = K - 273.15 (K - Kelvin, C - Celsius)
                double grip = (double)node.Attribute("y") * 100; // no further formatting for now, but this represents the percentage
                
                returnInfo.tyreTempsCelcius_Outside.Add(temperature);
                returnInfo.tempsGripPercentage_Outside.Add(grip);
            }
            
            
            
            
            // Tyre wear details
            tyreDetailsXML = root.Descendants(ns + "Compound")
                .Where(compound => (string)compound.Attribute("name") == compoundName)
                .Descendants(ns + "WearGrip")
                .Descendants(ns + "SplineElement");
            
            foreach (XElement node in tyreDetailsXML)
            {
                double wearLevelPercentage = (double)node.Attribute("x") * 100;
                double gripLevelPercentage = (double)node.Attribute("y") * 100;
                
                returnInfo.tyreWearPercentage.Add(wearLevelPercentage);
                returnInfo.wearGripPercentage.Add(gripLevelPercentage);
            }
            
            
            
            
            // Tyre wetness details
            tyreDetailsXML = root.Descendants(ns + "Compound")
                .Where(compound => (string)compound.Attribute("name") == compoundName)
                .Descendants(ns + "WetnessGrip")
                .Descendants(ns + "SplineElement");
            
            foreach (XElement node in tyreDetailsXML)
            {
                double wetnessLevelPercentage = (double)node.Attribute("x") * 100;
                double gripLevelPercentage = (double)node.Attribute("y") * 100;
                
                returnInfo.wetnessPercentage.Add(wetnessLevelPercentage);
                returnInfo.wetnessGripPercentage.Add(gripLevelPercentage);
            }
            
            

            return returnInfo;
        }
        
        
        

        
        
        
        public static string GetXMLContent()
        {
            // Reading the .erp file
            //TODO provide a console input method
            
            MainViewModel.Open("C:\\Users\\borsu\\Desktop\\My coding stuff\\MyF1ErpReader\\src\\MyF1ErpReader\\F1Files\\f1_2023_vehicle_package\\tyrecompounds\\tyrecompounds.erp");

            // obtaining the first element, it is written in such a way that it can theoretically store multiple XML files but we only load and want the first(and only) one
            var readXmlFile = MainViewModel.XmlFilesWorkspace.XmlFiles[0];

            ErpFragment fragment = readXmlFile.Fragment;

            byte[] data = fragment.GetDataArray(decompress: true);
            string xmlContent = Encoding.UTF8.GetString(data);

            return xmlContent;
        }
        
        public static void ConvertCompoundNames(List<CompoundInfo> compoundsList)
        {
            foreach (var compound in compoundsList)
            {
                string lastFourCharacters = compound.compoundName.Substring(compound.compoundName.Length - 4);
        
                switch (lastFourCharacters)
                {
                    case "USOF":
                        compound.compoundName = "C5";
                        break;
                    case "SUPF":
                        compound.compoundName = "C4";
                        break;
                    case "SOFF":
                        compound.compoundName = "C3";
                        break;
                    case "MEDF":
                        compound.compoundName = "C2";
                        break;
                    case "HARF":
                        compound.compoundName = "C1";
                        break;
                    case "SHAF":
                        compound.compoundName = "C0";
                        break;
                    case "INTF":
                        compound.compoundName = "INTERS";
                        compound.isWetOrOther = true;
                        break;
                    case "WETF":
                        compound.compoundName = "WETS";
                        compound.isWetOrOther = true;
                        break;
                    default:
                        compound.isWetOrOther = true;
                        break;
                }
            }
        }
    }
}