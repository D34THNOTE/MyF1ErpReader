namespace MyF1ErpReader.Models;

public class CompoundInfo
{
    public string compoundName { get; set; }
    
    public List<double> tyreTemps { get; set; } = new List<double>();
    public List<double> tempsGrip { get; set; } = new List<double>();
    
    
}