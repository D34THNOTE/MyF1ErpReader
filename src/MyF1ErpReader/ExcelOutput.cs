using MyF1ErpReader.Models;

using OfficeOpenXml;

namespace MyF1ErpReader;

public class ExcelOutput
{
    public static void GenerateExcelOutput(List<CompoundInfo> compoundInfoList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Create a new Excel package
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Output");

                int currentRow = 3;
                int currentColumn = 1;

                List<CompoundInfo> sortedCompoundInfoList = SortCompoundList(compoundInfoList);

                // Printing inside tyres' temperatures and grips
                worksheet.Cells[currentRow - 2, currentColumn].Value = "Temperatures for inside tyre";
                foreach (CompoundInfo compound in sortedCompoundInfoList)
                {
                    worksheet.Cells[currentRow - 1, currentColumn].Value = compound.compoundName;
                    worksheet.Cells[currentRow, currentColumn].Value = "Range(°C)";
                    worksheet.Cells[currentRow, currentColumn + 1].Value = "Grip(%)";
                    
                    for (int i = 0; i < compound.tyreTempsCelcius_Inside.Count; i++)
                    {
                        worksheet.Cells[currentRow + 1 + i, currentColumn].Value = compound.tyreTempsCelcius_Inside[i];
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = compound.tempsGripPercentage_Inside[i];
                    }

                    currentColumn += 3;
                }
                
                currentColumn = 1;
                currentRow += 1 + sortedCompoundInfoList[0].tyreTempsCelcius_Inside.Count + 3;
                
                // Printing outside tyres' temperatures and grips
                worksheet.Cells[currentRow - 2, currentColumn].Value = "Temperatures for outside tyre";
                foreach (CompoundInfo compound in sortedCompoundInfoList)
                {
                    worksheet.Cells[currentRow - 1, currentColumn].Value = compound.compoundName;
                    worksheet.Cells[currentRow, currentColumn].Value = "Range(°C)";
                    worksheet.Cells[currentRow, currentColumn + 1].Value = "Grip(%)";
                    
                    for (int i = 0; i < compound.tyreTempsCelcius_Outside.Count; i++)
                    {
                        worksheet.Cells[currentRow + 1 + i, currentColumn].Value = compound.tyreTempsCelcius_Outside[i];
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = compound.tempsGripPercentage_Outside[i];
                    }

                    currentColumn += 3;
                }
                
                currentColumn = 1;
                currentRow += 1 + sortedCompoundInfoList[0].tyreTempsCelcius_Outside.Count + 3;
                
                // Base % grip(first row) and its degradation due to wear
                worksheet.Cells[currentRow - 2, currentColumn].Value = 
                    "Base % grip(first row) for each tyre and its grip affected by their degradation(wear). " +
                    "Base % means the values are in relation to an \"objective\" scale, So C1 might have 90% grip while C5 has 98%";
                foreach (CompoundInfo compound in sortedCompoundInfoList)
                {
                    worksheet.Cells[currentRow - 1, currentColumn].Value = compound.compoundName;
                    worksheet.Cells[currentRow, currentColumn].Value = "Wear(%)";
                    worksheet.Cells[currentRow, currentColumn + 1].Value = "Grip(%)";
                    
                    for (int i = 0; i < compound.wearGripPercentage.Count; i++)
                    {
                        worksheet.Cells[currentRow + 1 + i, currentColumn].Value = compound.tyreWearPercentage[i];
                        
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = 
                            compound.wetnessGripPercentage[0] * (compound.wearGripPercentage[i]/100);
                    }

                    currentColumn += 3;
                }
                
                currentColumn = 1;
                currentRow += 1 + sortedCompoundInfoList[0].wearGripPercentage.Count + 3;
                
                // Base % grip(first row) and its degradation due to wear
                worksheet.Cells[currentRow - 2, currentColumn].Value = 
                    "Tyre wear usage - where 100% means 100% of the maximum grip for each tyre respectively";
                foreach (CompoundInfo compound in sortedCompoundInfoList)
                {
                    worksheet.Cells[currentRow - 1, currentColumn].Value = compound.compoundName;
                    worksheet.Cells[currentRow, currentColumn].Value = "Wear(%)";
                    worksheet.Cells[currentRow, currentColumn + 1].Value = "Grip(%)";
                    
                    for (int i = 0; i < compound.wearGripPercentage.Count; i++)
                    {
                        worksheet.Cells[currentRow + 1 + i, currentColumn].Value = compound.tyreWearPercentage[i];
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = compound.wearGripPercentage[i];
                    }

                    currentColumn += 3;
                }
                
                
                SaveExcelOutput(excelPackage);
            }
        }

    private static void SaveExcelOutput(ExcelPackage excelPackage)
    {
        string outputPath = "output.xlsx";
        FileInfo outputFileInfo = new FileInfo(outputPath);
        excelPackage.SaveAs(outputFileInfo);
    }

    private static List<CompoundInfo> SortCompoundList(List<CompoundInfo> compoundInfos)
    {
        List<CompoundInfo> returnList = new List<CompoundInfo>();
        List<CompoundInfo> outcastsList = new List<CompoundInfo>();

        CompoundInfo C0 = null;
        CompoundInfo C1 = null;
        CompoundInfo C2 = null;
        CompoundInfo C3 = null;
        CompoundInfo C4 = null;
        CompoundInfo C5 = null;
        CompoundInfo INTERS = null;
        CompoundInfo WETS = null;

        foreach (var compound in compoundInfos)
        {
            if (compound.compoundName.Equals("C0"))
            {
                C0 = compound;
            } 
            else if (compound.compoundName.Equals("C1"))
            {
                C1 = compound;
            } 
            else if (compound.compoundName.Equals("C2"))
            {
                C2 = compound;
            } 
            else if (compound.compoundName.Equals("C3"))
            {
                C3 = compound;
            } 
            else if (compound.compoundName.Equals("C4"))
            {
                C4 = compound;
            } 
            else if (compound.compoundName.Equals("C5"))
            {
                C5 = compound;
            } 
            else if (compound.compoundName.Equals("INTERS"))
            {
                INTERS = compound;
            } 
            else if (compound.compoundName.Equals("WETS"))
            {
                WETS = compound;
            }
            else
            {
                outcastsList.Add(compound);
            }
        }

        if (C0 != null &&
            C1 != null &&
            C2 != null &&
            C3 != null &&
            C4 != null &&
            C5 != null &&
            INTERS != null &&
            WETS != null)
        {
            C0.compoundName = "C0(F1 23)";
            returnList.Add(C0);
            returnList.Add(C1);
            returnList.Add(C2);
            returnList.Add(C3);
            returnList.Add(C4);
            returnList.Add(C5);
            returnList.Add(INTERS);
            returnList.Add(WETS);
        }
        else
        {
            throw new Exception("One of the tyre compounds was not found when executing SortCompoundList method");
        }

        if (outcastsList.Count > 0)
        {
            foreach (var compound in outcastsList)
            {
                returnList.Add(compound);
            }
        }

        return returnList;
    }

}