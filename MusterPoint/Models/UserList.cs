using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusterPoint.Models;

[BsonIgnoreExtraElements]
public class UserList
{
  [BsonId]
  [BsonRepresentation(BsonType.String)]
  public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
    public string Detachment { get; set; } = string.Empty;
    public int ArmyPoints { get; set; }
    public int GameSize { get; set; } = 2000;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ListEntry> Entries { get; set; } = new();

    public int CalculateTotalPoints()
    {
  return Entries.Sum(e => e.CalculatePoints());
    }
}

[BsonIgnoreExtraElements]
public class ListEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UnitId { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public int BasePoints { get; set; }
    public int MinSize { get; set; } = 1;
    public int MaxSize { get; set; } = 1;
    
    // MongoDB will serialize/deserialize this as ModelCount
    [BsonElement("ModelCount")]
    public int ModelCount { get; set; } = 1;
    
    public List<string> SelectedOptions { get; set; } = new();
    public string? Enhancement { get; set; } = null;
    public string? EnhancementName { get; set; } = null;
    public int EnhancementPoints { get; set; } = 0;
    public string? EnhancementDescription { get; set; } = null;
    public bool IsWarlord { get; set; } = false;

    public int CalculatePoints()
    {
        // ModelCount represents the actual number of models
  // Each "squad unit" costs BasePoints and covers MinSize to MaxSize models
  // Example: Intercessor Squad (80 pts, min 5, max 10)
        // 5 models = 1 squad = 80 pts
 // 6-10 models = 2 squads = 160 pts
 
 int squadsNeeded = (ModelCount - 1) / MinSize + 1;
   int totalPoints = BasePoints * squadsNeeded;
    
     // Add enhancement points
 if (!string.IsNullOrEmpty(Enhancement))
  {
         totalPoints += EnhancementPoints;
    }
        
      return totalPoints;
    }
}
