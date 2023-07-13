using System.Text;
using System.Xml;
using System.Xml.Linq;

using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;

using EgoEngineLibrary.Xml;

using EgoErpArchiver.ViewModel;

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
            
            GenerateExcelOutput(tyresDetailsList);


            
            /*
            CompoundInfo processCompoundInfo = tyresDetailsList[0];
            
            Console.WriteLine("Compound name: " + processCompoundInfo.compoundName + "\n");

            // Printing the extracted values
            for (int i = 0; i < processCompoundInfo.tyreTempsCelcius_Inside.Count; i++)
            {
                Console.WriteLine($"Inside Tyre Temperature: {processCompoundInfo.tyreTempsCelcius_Inside[i]:0.00} °C, " +
                                  $"Grip: {processCompoundInfo.tempsGripPercentage_Inside[i]:0.00}%");
                
                if(i+1 == processCompoundInfo.tyreTempsCelcius_Inside.Count) Console.WriteLine();
            }
            
            for (int i = 0; i < processCompoundInfo.tyreTempsCelcius_Outside.Count; i++)
            {
                Console.WriteLine($"Outside Tyre Temperature: {processCompoundInfo.tyreTempsCelcius_Outside[i]:0.00} °C, " +
                                  $"Grip: {processCompoundInfo.tempsGripPercentage_Outside[i]:0.00}%");
                
                if(i+1 == processCompoundInfo.tyreTempsCelcius_Outside.Count) Console.WriteLine();
            }
            
            for (int i = 0; i < processCompoundInfo.tyreWearPercentage.Count; i++)
            {
                Console.WriteLine($"Tyre Wear Level: {processCompoundInfo.tyreWearPercentage[i]:0.00}%, " +
                                  $"Grip Level: {processCompoundInfo.wearGripPercentage[i]:0.00}%");
                
                if(i+1 == processCompoundInfo.tyreWearPercentage.Count) Console.WriteLine();
            }
            
            for (int i = 0; i < processCompoundInfo.wetnessPercentage.Count; i++)
            {
                Console.WriteLine($"Tyre Wetness Level: {processCompoundInfo.wetnessPercentage[i]:0.00}%, " +
                                  $"Grip Level: {processCompoundInfo.wetnessGripPercentage[i]:0.00}%");
                
                if(i+1 == processCompoundInfo.wetnessPercentage.Count) Console.WriteLine();
            }
            
            */
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
        
        
        public static void GenerateExcelOutput(List<CompoundInfo> compoundInfoList)
        {
            // Create a new Excel package
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                // Create the worksheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Output");

                // Track the current row and column indices
                int currentRow = 1;
                int currentColumn = 1;

                // Write the header for the temperature range
                worksheet.Cells[currentRow, currentColumn].Value = "Temperature Range (°C)";
                currentColumn++;

                // Get the unique temperature values from the first CompoundInfo object
                List<double> temperatureRange = compoundInfoList[0].tyreTempsCelcius_Inside;

                // Write the temperature range values in the first column
                for (int i = 0; i < temperatureRange.Count; i++)
                {
                    worksheet.Cells[currentRow + i, currentColumn].Value = temperatureRange[i];
                }

                // Write the compound names in the first row
                for (int i = 0; i < compoundInfoList.Count; i++)
                {
                    worksheet.Cells[currentRow, currentColumn + i].Value = compoundInfoList[i].compoundName;
                }

                // Move to the next row
                currentRow++;

                // Write the grip values for each compound
                for (int i = 0; i < compoundInfoList.Count; i++)
                {
                    currentColumn = 2; // Start from the second column

                    // Write the grip values for tempsGripPercentage_Inside
                    List<double> gripValues = compoundInfoList[i].tempsGripPercentage_Inside;
                    for (int j = 0; j < gripValues.Count; j++)
                    {
                        worksheet.Cells[currentRow + j, currentColumn + i].Value = gripValues[j];
                    }
                }

                // Save the Excel package to a file
                string outputPath = "output.xlsx";
                FileInfo outputFileInfo = new FileInfo(outputPath);
                excelPackage.SaveAs(outputFileInfo);
            }
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