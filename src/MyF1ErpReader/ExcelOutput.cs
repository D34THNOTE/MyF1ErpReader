using System.Drawing;

using MyF1ErpReader.Models;

using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Style;

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
                    
                    double maxValLeftCol = Double.MinValue, maxValRightCol = Double.MinValue, minValLeftCol = Double.MaxValue, minValRightCol = Double.MaxValue;
                        
                    int optimalRangeStartIndx = -1;
                    int optimalRangeEndIndx = -1;
                    
                    for (int i = 0; i < compound.tyreTempsCelcius_Inside.Count; i++)
                    {
                        worksheet.Cells[currentRow + 1 + i, currentColumn].Value = compound.tyreTempsCelcius_Inside[i];
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = compound.tempsGripPercentage_Inside[i];

                        if (maxValLeftCol < compound.tyreTempsCelcius_Inside[i])
                            maxValLeftCol = compound.tyreTempsCelcius_Inside[i];
                        
                        if (minValLeftCol > compound.tyreTempsCelcius_Inside[i])
                            minValLeftCol = compound.tyreTempsCelcius_Inside[i];
                        
                        if (maxValRightCol < compound.tempsGripPercentage_Inside[i])
                            maxValRightCol = compound.tempsGripPercentage_Inside[i];
                        
                        if (minValRightCol > compound.tempsGripPercentage_Inside[i])
                            minValRightCol = compound.tempsGripPercentage_Inside[i];
                        
                        // saving indexes of both ends of optimal tyre ranges
                        if (i != 0 &&
                            compound.tempsGripPercentage_Inside[i].Equals(100) &&
                            !compound.tempsGripPercentage_Inside[i - 1].Equals(100)) optimalRangeStartIndx = i;

                        if (i + 1 != compound.tyreTempsCelcius_Inside.Count &&
                            compound.tempsGripPercentage_Inside[i].Equals(100) &&
                            !compound.tempsGripPercentage_Inside[i + 1].Equals(100)) optimalRangeEndIndx = i;
                    }

                    if (optimalRangeStartIndx == -1 || optimalRangeEndIndx == -1) throw new Exception("One of the optimal indexes has an invalid value");

                    double midValLeftCol = compound.tyreTempsCelcius_Inside[optimalRangeStartIndx] + 
                                           (
                                               (compound.tyreTempsCelcius_Inside[optimalRangeEndIndx] - 
                                                compound.tyreTempsCelcius_Inside[optimalRangeStartIndx])/2 
                                           );
                    
                    double midValRightCol = minValRightCol + ( (maxValRightCol - minValRightCol)/2 );

                    ColorScaleOneCompound(worksheet, currentRow+1, currentRow + compound.tyreTempsCelcius_Inside.Count,
                        currentColumn, minValLeftCol, midValLeftCol, maxValLeftCol,
                        Color.CornflowerBlue, Color.MediumSeaGreen, Color.LightCoral, true);
                    
                    ColorScaleOneCompound(worksheet, currentRow+1, currentRow + compound.tyreTempsCelcius_Inside.Count,
                        currentColumn+1, minValRightCol, midValRightCol, maxValRightCol, 
                        Color.LightCoral, ColorTranslator.FromHtml("#e6dd40"), Color.MediumSeaGreen, false);

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
                    
                    double maxValLeftCol = Double.MinValue, maxValRightCol = Double.MinValue, minValLeftCol = Double.MaxValue, minValRightCol = Double.MaxValue;
                        
                    int optimalRangeStartIndx = -1;
                    int optimalRangeEndIndx = -1;
                    
                    for (int i = 0; i < compound.tyreTempsCelcius_Outside.Count; i++)
                    {
                        worksheet.Cells[currentRow + 1 + i, currentColumn].Value = compound.tyreTempsCelcius_Outside[i];
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = compound.tempsGripPercentage_Outside[i];
                        
                        if (maxValLeftCol < compound.tyreTempsCelcius_Outside[i])
                            maxValLeftCol = compound.tyreTempsCelcius_Outside[i];
                        
                        if (minValLeftCol > compound.tyreTempsCelcius_Outside[i])
                            minValLeftCol = compound.tyreTempsCelcius_Outside[i];
                        
                        if (maxValRightCol < compound.tempsGripPercentage_Outside[i])
                            maxValRightCol = compound.tempsGripPercentage_Outside[i];
                        
                        if (minValRightCol > compound.tempsGripPercentage_Outside[i])
                            minValRightCol = compound.tempsGripPercentage_Outside[i];
                        
                        // saving indexes of both ends of optimal tyre ranges
                        if (i != 0 &&
                            compound.tempsGripPercentage_Outside[i].Equals(100) &&
                            !compound.tempsGripPercentage_Outside[i - 1].Equals(100)) optimalRangeStartIndx = i;

                        if (i + 1 != compound.tyreTempsCelcius_Outside.Count &&
                            compound.tempsGripPercentage_Outside[i].Equals(100) &&
                            !compound.tempsGripPercentage_Outside[i + 1].Equals(100)) optimalRangeEndIndx = i;
                    }
                    
                    if (optimalRangeStartIndx == -1 || optimalRangeEndIndx == -1) throw new Exception("One of the optimal indexes has an invalid value");

                    double midValLeftCol = compound.tyreTempsCelcius_Outside[optimalRangeStartIndx] + 
                                           (
                                               (compound.tyreTempsCelcius_Outside[optimalRangeEndIndx] - 
                                                compound.tyreTempsCelcius_Outside[optimalRangeStartIndx])/2 
                                           );
                    
                    double midValRightCol = minValRightCol + ( (maxValRightCol - minValRightCol)/2 );

                    ColorScaleOneCompound(worksheet, currentRow+1, currentRow + compound.tyreTempsCelcius_Outside.Count,
                        currentColumn, minValLeftCol, midValLeftCol, maxValLeftCol,
                        Color.CornflowerBlue, Color.MediumSeaGreen, Color.LightCoral, true);
                    
                    ColorScaleOneCompound(worksheet, currentRow+1, currentRow + compound.tyreTempsCelcius_Outside.Count,
                        currentColumn+1, minValRightCol, midValRightCol, maxValRightCol, 
                        Color.LightCoral, ColorTranslator.FromHtml("#e6dd40"), Color.MediumSeaGreen, false);

                    currentColumn += 3;
                }
                
                currentColumn = 1;
                currentRow += 1 + sortedCompoundInfoList[0].tyreTempsCelcius_Outside.Count + 3;
                
                
                
                
                // Base % grip(first row) and its degradation due to wear
                worksheet.Cells[currentRow - 2, currentColumn].Value = 
                    "Base % grip(first row) for each tyre and its grip affected by their degradation(wear). " +
                    "Base % means the values are in relation to an \"objective\" scale, So C1 might have 90% grip while C5 has 98%";
                
                double maxValObjectiveGrip = Double.MinValue, minValObjectiveGrip = Double.MaxValue;
                int numberOfNonWetsOrOthers = 0;
                
                foreach (CompoundInfo compound in sortedCompoundInfoList)
                {
                    worksheet.Cells[currentRow - 1, currentColumn].Value = compound.compoundName;
                    worksheet.Cells[currentRow, currentColumn].Value = "Wear(%)";
                    worksheet.Cells[currentRow, currentColumn + 1].Value = "Grip(%)";
                    
                    double maxValLeftCol = Double.MinValue, maxValRightCol = Double.MinValue, minValLeftCol = Double.MaxValue, minValRightCol = Double.MaxValue;
                    
                    for (int i = 0; i < compound.wearGripPercentage.Count; i++)
                    {
                        worksheet.Cells[currentRow + 1 + i, currentColumn].Value = compound.tyreWearPercentage[i];

                        double objectiveGrip = compound.wetnessGripPercentage[0] * (compound.wearGripPercentage[i] / 100);
                        
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = objectiveGrip;

                        if (maxValLeftCol < compound.tyreWearPercentage[i])
                            maxValLeftCol = compound.tyreWearPercentage[i];
                        
                        if (minValLeftCol > compound.tyreWearPercentage[i])
                            minValLeftCol = compound.tyreWearPercentage[i];
                        
                        // creating separate scales for wet and unrecognized compounds, C0, C1..C5 compounds should have a scale together since it's "objective" - in the
                        // same "solution space", on the same scale
                        if (compound.isWetOrOther)
                        {
                            if (maxValRightCol < objectiveGrip)
                                maxValRightCol = objectiveGrip;
                        
                            if (minValRightCol > objectiveGrip)
                                minValRightCol = objectiveGrip;
                        }
                        
                        if (!compound.isWetOrOther)
                        {
                            if (maxValObjectiveGrip < objectiveGrip)
                                maxValObjectiveGrip = objectiveGrip;
                        
                            if (minValObjectiveGrip > objectiveGrip)
                                minValObjectiveGrip = objectiveGrip;
                        }
                    }
                    
                    double midValLeftCol = maxValLeftCol / 2;
                    
                    ColorScaleOneCompound(worksheet, currentRow+1, currentRow + compound.wearGripPercentage.Count,
                        currentColumn, minValLeftCol, midValLeftCol, maxValLeftCol,
                        Color.MediumSeaGreen, Color.Yellow, Color.LightCoral, true);

                    if (compound.isWetOrOther)
                    {
                        double midValRightCol = minValRightCol + ( (maxValRightCol - minValRightCol)/2 );

                        ColorScaleOneCompound(worksheet, currentRow+1, currentRow + compound.wearGripPercentage.Count,
                            currentColumn+1, minValRightCol, midValRightCol, maxValRightCol, 
                            Color.LightCoral, ColorTranslator.FromHtml("#e6dd40"), Color.MediumSeaGreen, false);
                    }
                    else
                    {
                        numberOfNonWetsOrOthers++;
                    }

                    currentColumn += 3;
                }

                currentColumn = 1;
                double midValObjectiveGrip = minValObjectiveGrip + ( (maxValObjectiveGrip - minValObjectiveGrip)/2 );
                for (int i = 0; i < numberOfNonWetsOrOthers; i++)
                {
                    ColorScaleOneCompound(worksheet, currentRow+1, currentRow + sortedCompoundInfoList[0].wearGripPercentage.Count,
                        currentColumn+1 + (i*3), minValObjectiveGrip, midValObjectiveGrip, maxValObjectiveGrip, 
                        Color.LightCoral, ColorTranslator.FromHtml("#e6dd40"), Color.MediumSeaGreen, false);
                }
                
                
                currentColumn = 1;
                currentRow += 1 + sortedCompoundInfoList[0].wearGripPercentage.Count + 3;
                
                
                
                
                
                // same table but with C-type tyres close together for improved readability
                worksheet.Cells[currentRow - 2, currentColumn].Value = 
                    "Same thing as the table above but with C-compounds grouped together for improved readability";
                
                double maxValObjectiveGripGrouped = Double.MinValue, minValObjectiveGripGrouped = Double.MaxValue;
                int numberOfNonWetsOrOthersGrouped = 0;

                // printing tyre wear scale
                worksheet.Cells[currentRow - 1, currentColumn].Value = "Wear(%)";
                double maxTempWear = Double.MinValue, minTempWear = Double.MaxValue;
                for (int i = 0; i < sortedCompoundInfoList[0].wearGripPercentage.Count; i++)
                {
                    worksheet.Cells[currentRow + 1 + i, currentColumn].Value = sortedCompoundInfoList[0].tyreWearPercentage[i];

                    if (maxTempWear < sortedCompoundInfoList[0].tyreWearPercentage[i])
                        maxTempWear = sortedCompoundInfoList[0].tyreWearPercentage[i];
                    
                    if (minTempWear > sortedCompoundInfoList[0].tyreWearPercentage[i])
                        minTempWear = sortedCompoundInfoList[0].tyreWearPercentage[i];
                }

                double midTempWear = minTempWear + ((maxTempWear - minTempWear) / 2); 
                ColorScaleOneCompound(worksheet, currentRow+1, 
                    currentRow + sortedCompoundInfoList[0].wearGripPercentage.Count,
                    currentColumn, minTempWear, midTempWear, maxTempWear, 
                    Color.MediumSeaGreen, Color.Yellow, Color.LightCoral, true);
                
                
                
                foreach (CompoundInfo compound in sortedCompoundInfoList)
                {
                    if (compound.isWetOrOther) break;
                    else numberOfNonWetsOrOthersGrouped++;
                    
                    worksheet.Cells[currentRow, currentColumn + 1].Value = compound.compoundName;
                    worksheet.Cells[currentRow - 1, currentColumn + 1].Value = "Grip(%)";

                    for (int i = 0; i < compound.wearGripPercentage.Count; i++)
                    {
                        double objectiveGrip = compound.wetnessGripPercentage[0] * (compound.wearGripPercentage[i] / 100);
                        
                        worksheet.Cells[currentRow + 1 + i, currentColumn + 1].Value = objectiveGrip;

                        if (maxValObjectiveGripGrouped < objectiveGrip)
                            maxValObjectiveGripGrouped = objectiveGrip;
                        
                        if (minValObjectiveGripGrouped > objectiveGrip)
                            minValObjectiveGripGrouped = objectiveGrip;
                    }
                    
                    currentColumn += 1;
                }
                
                currentColumn = 1;
                double midValObjectiveGripGrouped = minValObjectiveGripGrouped + ( (maxValObjectiveGripGrouped - minValObjectiveGripGrouped)/2 );
                for (int i = 0; i < numberOfNonWetsOrOthers; i++)
                {
                    ColorScaleOneCompound(worksheet, currentRow+1, currentRow + sortedCompoundInfoList[0].wearGripPercentage.Count,
                        currentColumn+1 + i, minValObjectiveGripGrouped, midValObjectiveGripGrouped, maxValObjectiveGripGrouped, 
                        Color.LightCoral, ColorTranslator.FromHtml("#e6dd40"), Color.MediumSeaGreen, false);
                }
                
                currentColumn = 1;
                currentRow += 1 + sortedCompoundInfoList[0].wearGripPercentage.Count + 3;
                
                
                
                /*
                
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
                */
                
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

    private static void ColorScaleOneCompound(ExcelWorksheet worksheet, int startRow, int endRow, int column, double minVal, double midVal, double maxVal, 
        Color colorLow, Color colorMid, Color colorHigh,
        bool isLeftCol)
    {
        string colLetter = ColNumberToExcelLetter(column);
        string scaleRangeToPaint = colLetter + startRow + ":" + colLetter + endRow;
        
        var conditionalFormatting = worksheet.ConditionalFormatting.AddThreeColorScale(worksheet.Cells[scaleRangeToPaint]);

        // Set the conditional formatting properties
        conditionalFormatting.LowValue.Type = eExcelConditionalFormattingValueObjectType.Num;
        conditionalFormatting.LowValue.Value = minVal;
        conditionalFormatting.LowValue.Color = colorLow;
                
        conditionalFormatting.MiddleValue.Type = eExcelConditionalFormattingValueObjectType.Num;
        conditionalFormatting.MiddleValue.Value = midVal;
        conditionalFormatting.MiddleValue.Color = colorMid;
                
        conditionalFormatting.HighValue.Type = eExcelConditionalFormattingValueObjectType.Num;
        conditionalFormatting.HighValue.Value = maxVal;
        conditionalFormatting.HighValue.Color = colorHigh;
        
        // Adding borders
        ExcelRange range = worksheet.Cells[scaleRangeToPaint];

        if (isLeftCol)
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Medium;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;   
        }
        else
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
    }

    
    private static string ColNumberToExcelLetter(int column)
    {
        if (column < 1)
            throw new ArgumentException("Column number must be greater than 0.");

        string excelLetter = "";

        while (column > 0)
        {
            int remainder = (column - 1) % 26;
            char letter = (char)('A' + remainder);
            excelLetter = letter + excelLetter;
            column = (column - 1) / 26;
        }

        return excelLetter;
    }

}