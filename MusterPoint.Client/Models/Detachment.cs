namespace MusterPoint.Client.Models;

public class Detachment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
    public string FactionKeyword { get; set; } = string.Empty;
    public DetachmentRule DetachmentRule { get; set; } = new();
  public List<Enhancement> Enhancements { get; set; } = new();
}

public class DetachmentRule
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class Enhancement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
 public int Points { get; set; }
    public string Restrictions { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
