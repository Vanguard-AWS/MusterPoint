using MusterPoint.Client.Models;
using System.Net.Http.Json;

namespace MusterPoint.Client.Services;

public class DatasheetService
{
    private readonly HttpClient _httpClient;
    private readonly BSdataService _bsdataService;
    private Dictionary<string, Unit>? _unitsCache;
    private List<string>? _factionsCache;

    public DatasheetService(HttpClient httpClient, BSdataService bsdataService)
    {
  _httpClient = httpClient;
  _bsdataService = bsdataService;
    }

    public async Task<List<Unit>> GetAllUnitsAsync()
    {
        if (_unitsCache != null)
  {
      Console.WriteLine($"[DatasheetService] Returning cached units ({_unitsCache.Count} units)");
  return _unitsCache.Values.ToList();
      }

    _unitsCache = new();

      try
 {
       Console.WriteLine($"[DatasheetService] Starting to load all units from BSdata");

        // Load from BSdata
      Console.WriteLine($"[DatasheetService] Fetching available factions from BSdata...");
          var factions = await _bsdataService.GetAvailableFactionsAsync();
     Console.WriteLine($"[DatasheetService] Loading units from {factions.Count} factions");

     foreach (var faction in factions)
          {
 try
   {
 Console.WriteLine($"[DatasheetService] --> Loading units for faction: {faction}");
         var factionUnits = await _bsdataService.GetUnitsByFactionAsync(faction);
  
       foreach (var unit in factionUnits)
    {
            _unitsCache[unit.Id] = unit;
      }
         
  Console.WriteLine($"[DatasheetService] --> Successfully loaded {factionUnits.Count} units for {faction}");
   }
        catch (Exception ex)
   {
 Console.WriteLine($"[DatasheetService] Error loading units for {faction} from BSdata: {ex.Message}");
  }
        }

     Console.WriteLine($"[DatasheetService] All factions loaded. Total units in cache: {_unitsCache.Count}");
  }
        catch (Exception ex)
        {
  Console.WriteLine($"[DatasheetService] Error loading datasheets: {ex.Message}");
    Console.WriteLine($"[DatasheetService] Stack trace: {ex.StackTrace}");
  }

        return _unitsCache.Values.ToList();
    }

    public async Task<Unit?> GetUnitByIdAsync(string unitId)
    {
       if (_unitsCache == null)
          await GetAllUnitsAsync();

        var result = _unitsCache?.TryGetValue(unitId, out var unit) == true ? unit : null;
      Console.WriteLine($"[DatasheetService] GetUnitByIdAsync: {unitId} -> {(result != null ? result.Name : "NOT FOUND")}");
      return result;
    }

    public async Task<List<Unit>> GetUnitsByFactionAsync(string faction)
    {
 Console.WriteLine($"[DatasheetService] GetUnitsByFactionAsync: {faction}");
        var result = await _bsdataService.GetUnitsByFactionAsync(faction);
       Console.WriteLine($"[DatasheetService] Found {result.Count} units for faction: {faction}");
        return result;
   }

    public async Task<List<string>> GetFactionsAsync()
    {
        if (_factionsCache != null)
        {
       Console.WriteLine($"[DatasheetService] Returning cached factions ({_factionsCache.Count} factions)");
     return _factionsCache;
       }

        Console.WriteLine($"[DatasheetService] Loading factions from BSdata");
     _factionsCache = await _bsdataService.GetAvailableFactionsAsync();

    Console.WriteLine($"[DatasheetService] Factions loaded: {string.Join(", ", _factionsCache.Take(5))}...");
 return _factionsCache;
}

    public async Task<List<Unit>> SearchUnitsAsync(string searchTerm)
    {
        Console.WriteLine($"[DatasheetService] SearchUnitsAsync: '{searchTerm}'");
  var allUnits = await GetAllUnitsAsync();
        var lowerSearch = searchTerm.ToLower();
       var results = allUnits
            .Where(u => u.Name.ToLower().Contains(lowerSearch) || u.Faction.ToLower().Contains(lowerSearch))
           .ToList();
  
     Console.WriteLine($"[DatasheetService] Search found {results.Count} results for '{searchTerm}'");
  return results;
    }
}

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

public class FactionUnitIndex
{
    public string Faction { get; set; } = string.Empty;
    public List<UnitRef>? Units { get; set; }
}

public class UnitRef
{
    public string Ref { get; set; } = string.Empty;
}
