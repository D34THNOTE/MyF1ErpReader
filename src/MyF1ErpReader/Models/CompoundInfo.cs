namespace MyF1ErpReader.Models;

public class CompoundInfo
{
    public string compoundName { get; set; }
    
    public List<double> tyreTempsCelcius_Inside { get; set; } = new List<double>();
    public List<double> tempsGripPercentage_Inside { get; set; } = new List<double>();
    
    public List<double> tyreTempsCelcius_Outside { get; set; } = new List<double>();
    public List<double> tempsGripPercentage_Outside { get; set; } = new List<double>();
    
    // tyreWearPercentage - a list of percentages representing different levels of wear, starting from 0% and going up to 100%
    public List<double> tyreWearPercentage { get; set; } = new List<double>();
    public List<double> wearGripPercentage { get; set; } = new List<double>();
    
    // This is for calculating real tyre grip % based on wear and its objective grip level
    // IMPORTANT: The first record in wetnessGripPercentage represents the objective tyre grip level, e.g. a fresh C5 tyre in most F1 games has 98% grip, C4 has 96% etc
    // Formula: Base % of grip(taken from 0% attribute of WetnessGrip) * % of relative grip left for each percentage level(from wearGripPercentage)
    public List<double> wetnessPercentage { get; set; } = new List<double>();
    public List<double> wetnessGripPercentage { get; set; } = new List<double>();
}