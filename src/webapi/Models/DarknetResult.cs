namespace Digitalist.ObjectRecognition.Models
{
  public class DarknetResult
  {
    public string Class { get; set; }
    public float Probability { get; set; }
    public BoundingBox BoundingBox { get; set; }
  }
}