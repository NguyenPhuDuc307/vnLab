namespace vnLab.Data.Entities;

public class Recommendation
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime Asked { get; set; }
    public DateTime Modified { get; set; }
    public int Viewed { get; set; }
    public string? Tags { get; set; }
    public double Score { get; set; }
}