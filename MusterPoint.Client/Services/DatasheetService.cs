using MusterPoint.Client.Models;
using System.Net.Http.Json;

namespace MusterPoint.Client.Services;

public class DatasheetService
{
    private readonly HttpClient _httpClient;
    private Dictionary<string, Unit>? _unitsCache;
    private List<string>? _factionsCache;
    private Dictionary<string, string>? _factionPathsCache;

    public DatasheetService(HttpClient httpClient)
    {
   _httpClient = httpClient;
    }

    /// <summary>
    /// Load faction index to get paths to each faction's unit file
    /// </summary>
    private async Task<Dictionary<string, string>> LoadFactionIndexAsync()
    {
        if (_factionPathsCache != null)
            return _factionPathsCache;

     _factionPathsCache = new();

        try
        {
            var factionIndex = await _httpClient.GetFromJsonAsync<FactionIndex>("data/factions-index.json");
 
            if (factionIndex?.Factions != null)
{
      foreach (var faction in factionIndex.Factions)
                {
   _factionPathsCache[faction.Name] = faction.Path;
  }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading faction index: {ex.Message}");
        }

        return _factionPathsCache;
    }

    public async Task<List<Unit>> GetAllUnitsAsync()
    {
    if (_unitsCache != null)
   return _unitsCache.Values.ToList();

_unitsCache = new();

 try
   {
    // Load faction index
         var factionPaths = await LoadFactionIndexAsync();

   // Load units from each faction's modular unit files
            foreach (var (factionName, factionIndexPath) in factionPaths)
   {
            try
      {
         // Load the faction's unit index (e.g., /data/factions/space-marines/index.json)
           var factionUnitIndex = await _httpClient.GetFromJsonAsync<FactionUnitIndex>(factionIndexPath);

      if (factionUnitIndex?.Units != null)
   {
           // Get the directory path from the faction index path
            var factionDir = factionIndexPath.Substring(0, factionIndexPath.LastIndexOf('/'));

// Load each unit category file referenced in the index
          foreach (var unitRef in factionUnitIndex.Units)
           {
        try
   {
       // Construct the full path to the category file
                 var categoryPath = $"{factionDir}/{unitRef.Ref}";
 var units = await _httpClient.GetFromJsonAsync<List<Unit>>(categoryPath);

           if (units != null)
           {
          foreach (var unit in units)
            {
      _unitsCache[unit.Id] = unit;
              }
          }
          }
 catch (Exception ex)
    {
            Console.WriteLine($"Error loading unit category {unitRef.Ref}: {ex.Message}");
   }
      }
                }
        }
          catch (Exception ex)
 {
    Console.WriteLine($"Error loading faction {factionName}: {ex.Message}");
         }
       }
        }
     catch (Exception ex)
        {
            Console.WriteLine($"Error loading datasheets: {ex.Message}");
        }

    return _unitsCache.Values.ToList();
    }

    public async Task<Unit?> GetUnitByIdAsync(string unitId)
    {
      if (_unitsCache == null)
        await GetAllUnitsAsync();

        return _unitsCache?.TryGetValue(unitId, out var unit) == true ? unit : null;
    }

    public async Task<List<Unit>> GetUnitsByFactionAsync(string faction)
    {
        var allUnits = await GetAllUnitsAsync();
        return allUnits.Where(u => u.Faction.Equals(faction, StringComparison.OrdinalIgnoreCase)).ToList();
  }

  public async Task<List<string>> GetFactionsAsync()
    {
        if (_factionsCache != null)
    return _factionsCache;

   var allUnits = await GetAllUnitsAsync();
   _factionsCache = allUnits
   .Select(u => u.Faction)
      .Distinct()
     .OrderBy(f => f)
.ToList();

      return _factionsCache;
    }

    public async Task<List<Unit>> SearchUnitsAsync(string searchTerm)
    {
   var allUnits = await GetAllUnitsAsync();
 var lowerSearch = searchTerm.ToLower();
    return allUnits
         .Where(u => u.Name.ToLower().Contains(lowerSearch) || u.Faction.ToLower().Contains(lowerSearch))
            .ToList();
    }
}

/// <summary>
/// Models for faction index deserialization
/// </summary>
public class FactionIndex
{
    public List<FactionEntry>? Factions { get; set; }
    public string? Description { get; set; }
}

public class FactionEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Model for faction unit index deserialization (e.g., data/factions/space-marines/index.json)
/// </summary>
public class FactionUnitIndex
{
    public string Faction { get; set; } = string.Empty;
    public List<UnitRef>? Units { get; set; }
}

public class UnitRef
{
    public string Ref { get; set; } = string.Empty;
}
