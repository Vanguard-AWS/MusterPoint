namespace MusterPoint.Client.Models;

public class Detachment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
    public string FactionKeyword { get; set; } = string.Empty;
    public DetachmentRule DetachmentRule { get; set; } = new();
    public List<Enhancement> Enhancements { get; set; } = new();
    
    /// <summary>
    /// List of subfaction IDs that this detachment requires.
    /// Empty list = available to all subfactions.
    /// Non-empty = only available when one of these subfactions is selected.
    /// </summary>
    public List<string> RequiredSubfactionIds { get; set; } = new();
    
    /// <summary>
    /// Human-readable subfaction names for display purposes
    /// </summary>
    public List<string> RequiredSubfactionNames { get; set; } = new();
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
