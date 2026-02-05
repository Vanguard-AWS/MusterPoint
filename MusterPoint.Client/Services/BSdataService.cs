using System.Xml.Linq;
using MusterPoint.Client.Models;
using System.Net.Http.Json;

namespace MusterPoint.Client.Services;

/// <summary>
/// Service to fetch and parse BSdata Warhammer 40K catalogues from GitHub via backend proxy.
/// Uses the /api/bsdataproxy endpoint to avoid CORS issues.
/// </summary>
public class BSdataService
{
    private readonly HttpClient _httpClient;
    private const string PROXY_URL = "/api/bsdataproxy";
    
    // Cache to avoid repeated requests
    private Dictionary<string, CatalogueCache> _catalogueCache = new();
    private List<string>? _factionsCache;
    
    public BSdataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches units for a specific faction from BSdata (via backend proxy)
    /// </summary>
    public async Task<List<Unit>> GetUnitsByFactionAsync(string factionName)
    {
   try
   {
    var catalogueXml = await FetchCatalogueAsync(factionName);
    if (catalogueXml == null)
  {
      return new();
    }

var units = ParseUnitsFromCatalogue(catalogueXml, factionName);
         
       return units;
        }
        catch (Exception ex)
        {
 return new();
        }
    }

  /// <summary>
  /// Gets available factions from BSdata
/// </summary>
    public async Task<List<string>> GetAvailableFactionsAsync()
    {
        if (_factionsCache != null)
      {
        return _factionsCache;
  }

      try
        {
            var factions = GetFactionList();
   _factionsCache = factions;
       return factions;
        }
        catch (Exception ex)
        {
    return new();
 }
    }

    /// <summary>
    /// Fetches and caches a catalogue XML for a faction via backend proxy
    /// </summary>
    private async Task<XDocument?> FetchCatalogueAsync(string factionName)
    {
        if (_catalogueCache.TryGetValue(factionName, out var cached))
        {
            if (DateTime.UtcNow - cached.CachedAt < TimeSpan.FromHours(24))
        {
    return cached.Document;
 }
  }

    try
    {
       var proxyUrl = $"{PROXY_URL}/catalogue?faction={Uri.EscapeDataString(factionName)}";
    
  var response = await _httpClient.GetAsync(proxyUrl);
    if (!response.IsSuccessStatusCode)
     {
         return null;
         }

        var proxyResponse = await response.Content.ReadFromJsonAsync<BSdataProxyResponse>();
   if (proxyResponse?.content == null)
     {
     return null;
     }

       var doc = XDocument.Parse(proxyResponse.content);
         
          // Cache the result
   _catalogueCache[factionName] = new CatalogueCache
      {
 Document = doc,
        CachedAt = DateTime.UtcNow
      };

            return doc;
    }
        catch (Exception ex)
    {
    return null;
}
    }

    private List<Unit> ParseUnitsFromCatalogue(XDocument doc, string faction)
    {
   var units = new List<Unit>();
      var ns = doc.Root?.Name.NamespaceName;
        var xns = ns != null ? XNamespace.Get(ns) : null;

   try
        {
         var selectionEntries = doc.Descendants(xns + "selectionEntry");

     int unitCount = 0;
         foreach (var entry in selectionEntries)
{
       try
      {
      var typeAttr = entry.Attribute("type")?.Value;
           if (typeAttr?.Contains("unit", StringComparison.OrdinalIgnoreCase) != true)
       continue;

       var unit = ParseUnitEntry(entry, faction, xns);
    if (unit != null)
    {
    units.Add(unit);
  unitCount++;
     }
   }
     catch (Exception ex)
   {
     }
            }
        }
        catch (Exception ex)
        {
        }

        return units;
    }

    private Unit? ParseUnitEntry(XElement entry, string faction, XNamespace? ns)
    {
  try
   {
            ns ??= entry.Name.NamespaceName;

        var id = entry.Attribute("id")?.Value ?? Guid.NewGuid().ToString();
  var name = entry.Attribute("name")?.Value ?? "Unknown";
   
   int basePoints = ExtractPointsCost(entry, ns);
            var category = DetermineCategoryFromEntry(entry, ns);

       var profiles = ParseProfiles(entry, ns);
       var rangedWeapons = ParseRangedWeapons(entry, ns);
 var meleeWeapons = ParseMeleeWeapons(entry, ns);
        var keywords = ParseKeywords(entry, ns);
      var abilities = ParseAbilities(entry, ns);
    var (minSize, maxSize) = ExtractUnitSize(entry, ns);

 return new Unit
   {
  Id = id,
     Name = name,
       Faction = faction,
     Category = category,
 BasePoints = basePoints,
     MinSize = minSize,
      MaxSize = maxSize,
  Profiles = profiles,
  RangedWeapons = rangedWeapons,
    MeleeWeapons = meleeWeapons,
       Keywords = keywords,
          Abilities = abilities,
        WargearOptions = new()
     };
     }
  catch (Exception ex)
   {
     return null;
        }
    }

    private int ExtractPointsCost(XElement entry, XNamespace? ns)
    {
        try
        {
          ns ??= entry.Name.NamespaceName;
      var costElement = entry.Descendants(ns + "cost")
    .FirstOrDefault(c => c.Attribute("name")?.Value == "pts");

          if (costElement != null && int.TryParse(costElement.Attribute("value")?.Value, out var points))
    return points;
   }
    catch (Exception ex)
     {
}

        return 0;
    }

    private List<UnitProfile> ParseProfiles(XElement entry, XNamespace? ns)
  {
        var profiles = new List<UnitProfile>();

     try
      {
        ns ??= entry.Name.NamespaceName;
   
       var profilesElement = entry.Element(ns + "profiles");
     if (profilesElement == null)
     return profiles;

         foreach (var profile in profilesElement.Elements(ns + "profile"))
  {
     try
    {
 var characs = profile.Element(ns + "characteristics");
          if (characs == null)
         continue;

        var profileName = profile.Attribute("name")?.Value ?? "Model";
      
  var unitProfile = new UnitProfile
        {
        Name = profileName,
    Movement = ExtractCharacteristic(characs, ns, "M"),
     Toughness = ParseIntCharacteristic(characs, ns, "T"),
    Save = ExtractCharacteristic(characs, ns, "Sv"),
          Wounds = ParseIntCharacteristic(characs, ns, "W"),
       Leadership = ExtractCharacteristic(characs, ns, "Ld"),
       ObjectiveControl = ParseIntCharacteristic(characs, ns, "OC")
       };

     profiles.Add(unitProfile);
    }
    catch (Exception ex)
     {
           }
    }
        }
        catch (Exception ex)
 {
      }

   return profiles;
    }

    private List<RangedWeapon> ParseRangedWeapons(XElement entry, XNamespace? ns)
  {
      var weapons = new List<RangedWeapon>();

        try
        {
      ns ??= entry.Name.NamespaceName;

    var profilesElement = entry.Element(ns + "profiles");
      if (profilesElement == null)
 return weapons;

  var weaponProfiles = profilesElement.Elements(ns + "profile")
    .Where(p => IsRangedWeaponProfile(p, ns));

   foreach (var weapon in weaponProfiles)
            {
       try
 {
              var characs = weapon.Element(ns + "characteristics");
            if (characs == null)
 continue;

           var rangedWeapon = new RangedWeapon
         {
          Name = weapon.Attribute("name")?.Value ?? "Weapon",
       Range = ExtractCharacteristic(characs, ns, "Range"),
         Attacks = ExtractAttacks(characs, ns),
    BallisticSkill = ExtractCharacteristic(characs, ns, "BS"),
      Strength = ParseIntCharacteristic(characs, ns, "S"),
    ArmorPenetration = ParseAPCharacteristic(characs, ns, "AP"),
   Damage = ExtractDamage(characs, ns),
  Type = ExtractCharacteristic(characs, ns, "Type")
   };

            weapons.Add(rangedWeapon);
     }
        catch (Exception ex)
   {
        }
       }
        }
   catch (Exception ex)
        {
        }

        return weapons;
    }

    private List<MeleeWeapon> ParseMeleeWeapons(XElement entry, XNamespace? ns)
{
     var weapons = new List<MeleeWeapon>();

   try
    {
     ns ??= entry.Name.NamespaceName;

            var profilesElement = entry.Element(ns + "profiles");
  if (profilesElement == null)
    return weapons;

  var weaponProfiles = profilesElement.Elements(ns + "profile")
 .Where(p => IsMeleeWeaponProfile(p, ns));

            foreach (var weapon in weaponProfiles)
            {
    try
           {
      var characs = weapon.Element(ns + "characteristics");
   if (characs == null)
       continue;

  var meleeWeapon = new MeleeWeapon
  {
            Name = weapon.Attribute("name")?.Value ?? "Weapon",
        Attacks = ParseIntCharacteristic(characs, ns, "A"),
   WeaponSkill = ExtractCharacteristic(characs, ns, "WS"),
    Strength = ParseIntCharacteristic(characs, ns, "S"),
    ArmorPenetration = ParseAPCharacteristic(characs, ns, "AP"),
    Damage = ExtractDamage(characs, ns)
     };

      weapons.Add(meleeWeapon);
   }
         catch (Exception ex)
   {
        }
}
  }
     catch (Exception ex)
        {
        }

    return weapons;
    }

    private List<string> ParseKeywords(XElement entry, XNamespace? ns)
    {
        var keywords = new List<string>();

        try
    {
      ns ??= entry.Name.NamespaceName;

    var profilesElement = entry.Element(ns + "profiles");
       if (profilesElement != null)
      {
         var keywordProfile = profilesElement.Elements(ns + "profile")
       .FirstOrDefault(p => p.Attribute("name")?.Value?.Contains("Keyword", StringComparison.OrdinalIgnoreCase) ?? false);

 if (keywordProfile != null)
       {
           var characs = keywordProfile.Element(ns + "characteristics");
   if (characs != null)
     {
         var keywordText = ExtractCharacteristic(characs, ns, "Keywords");
       if (!string.IsNullOrEmpty(keywordText))
     {
   keywords = keywordText.Split(',')
  .Select(k => k.Trim())
  .Where(k => !string.IsNullOrEmpty(k))
  .ToList();
    }
          }
   }
 }

         var categories = entry.Descendants(ns + "categoryLink")
 .Select(c => c.Attribute("name")?.Value)
      .Where(c => !string.IsNullOrEmpty(c))
  .ToList();

    keywords.AddRange(categories!);
        }
        catch (Exception ex)
        {
        }

  return keywords.Distinct().ToList();
    }

private List<Ability> ParseAbilities(XElement entry, XNamespace? ns)
    {
 var abilities = new List<Ability>();

 try
        {
ns ??= entry.Name.NamespaceName;

   var rulesElement = entry.Element(ns + "rules");
        if (rulesElement == null)
  return abilities;

 foreach (var rule in rulesElement.Elements(ns + "rule"))
     {
    try
     {
     var ability = new Ability
         {
 Name = rule.Attribute("name")?.Value ?? "Ability",
Description = rule.Element(ns + "description")?.Value ?? "",
          Type = rule.Attribute("hidden")?.Value == "true" ? "hidden" : "unit"
         };

     abilities.Add(ability);
       }
    catch (Exception ex)
       {
        }
         }
        }
        catch (Exception ex)
        {
  }

   return abilities;
    }

    private (int minSize, int maxSize) ExtractUnitSize(XElement entry, XNamespace? ns)
{
        try
        {
     ns ??= entry.Name.NamespaceName;

      var entryLinks = entry.Elements(ns + "entryLink");
    int minSize = 1;
            int maxSize = 10;

            if (entryLinks.Any())
     {
      var counts = entryLinks
        .SelectMany(el => el.Descendants(ns + "constraint"))
 .Where(c => c.Attribute("field")?.Value == "selections")
     .Select(c => new
      {
 Type = c.Attribute("type")?.Value,
  Value = c.Attribute("value")?.Value
      })
     .ToList();

        var minConstraint = counts.FirstOrDefault(c => c.Type == "min");
    var maxConstraint = counts.FirstOrDefault(c => c.Type == "max");

  if (minConstraint != null && int.TryParse(minConstraint.Value, out var min))
        minSize = min;

      if (maxConstraint != null && int.TryParse(maxConstraint.Value, out var max))
   maxSize = max;
 }

return (minSize, maxSize);
        }
  catch (Exception ex)
        {
      return (1, 10);
        }
    }

    private string DetermineCategoryFromEntry(XElement entry, XNamespace? ns)
    {
        try
{
   ns ??= entry.Name.NamespaceName;

    var keywords = ParseKeywords(entry, ns);
     var keywordString = string.Join(" ", keywords).ToUpperInvariant();

    if (keywordString.Contains("CHARACTER"))
        return "Characters";
  if (keywordString.Contains("BATTLELINE"))
         return "Battleline";
            if (keywordString.Contains("TRANSPORT"))
    return "Dedicated Transports";
          if (keywordString.Contains("FORTIFICATION"))
           return "Fortifications";
 if (keywordString.Contains("VEHICLE"))
     return "Vehicles";
            if (keywordString.Contains("MONSTER"))
       return "Monsters";

   return "Other";
  }
        catch
        {
            return "Other";
        }
}

  private string ExtractCharacteristic(XElement characs, XNamespace? ns, string charName)
    {
     try
     {
  ns ??= characs.Name.NamespaceName;
    return characs.Elements(ns + "characteristic")
       .FirstOrDefault(c => c.Attribute("name")?.Value == charName)
    ?.Value ?? "-";
        }
 catch
   {
     return "-";
      }
    }

    private int ParseIntCharacteristic(XElement characs, XNamespace? ns, string charName)
    {
var value = ExtractCharacteristic(characs, ns, charName);
      return int.TryParse(value, out var result) ? result : 0;
    }

    private int ParseAPCharacteristic(XElement characs, XNamespace? ns, string charName)
    {
        var value = ExtractCharacteristic(characs, ns, charName);
 
      if (value.StartsWith("-"))
     value = value.Substring(1);

   return int.TryParse(value, out var result) ? -result : 0;
  }

private object ExtractAttacks(XElement characs, XNamespace? ns)
  {
        var value = ExtractCharacteristic(characs, ns, "A");
  return int.TryParse(value, out var num) ? (object)num : value;
    }

    private object ExtractDamage(XElement characs, XNamespace? ns)
    {
   var value = ExtractCharacteristic(characs, ns, "D");
      return int.TryParse(value, out var num) ? (object)num : value;
 }

    private bool IsRangedWeaponProfile(XElement profile, XNamespace? ns)
    {
        try
        {
   ns ??= profile.Name.NamespaceName;
var characs = profile.Element(ns + "characteristics");
      
     return characs?.Elements(ns + "characteristic")
      .Any(c => 
 {
        var name = c.Attribute("name")?.Value ?? "";
      return name == "Range" || name == "BS";
       }) ?? false;
     }
  catch
 {
       return false;
   }
    }

 private bool IsMeleeWeaponProfile(XElement profile, XNamespace? ns)
    {
   try
        {
          ns ??= profile.Name.NamespaceName;
    var characs = profile.Element(ns + "characteristics");
            
  var hasWS = characs?.Elements(ns + "characteristic")
  .Any(c => c.Attribute("name")?.Value == "WS") ?? false;
     var hasRange = characs?.Elements(ns + "characteristic")
     .Any(c => c.Attribute("name")?.Value == "Range") ?? false;
      
 return hasWS && !hasRange;
        }
        catch
        {
return false;
    }
    }

    private List<string> GetFactionList()
    {
        return new()
        {
 // Imperium (19 factions)
      "Space Marines",
     "Blood Angels",
            "Dark Angels",
     "Deathwatch",
      "Imperial Fists",
       "Iron Hands",
     "Raven Guard",
    "Salamanders",
   "Space Wolves",
 "Ultramarines",
"White Scars",
    "Imperial Knights",
       "Adeptus Mechanicus",
     "Grey Knights",
        "Black Templars",
     "Astra Militarum",
         "Adepta Sororitas",
      "Adeptus Custodes",
            "Agents of the Imperium",

 // Chaos (7 factions)
     "Chaos Space Marines",
 "Chaos Daemons",
   "Death Guard",
        "Thousand Sons",
 "Chaos Knights",
 "Emperor's Children",
         "World Eaters",
     
 // Xenos (6 factions)
    "Necrons",
          "Orks",
   "T'au Empire",
      "Tyranids",
       "Leagues of Votann",
    "Aeldari",
    };
    }

    private class CatalogueCache
    {
        public XDocument Document { get; set; } = null!;
      public DateTime CachedAt { get; set; }
    }

    private class BSdataProxyResponse
    {
public string? faction { get; set; }
 public string? content { get; set; }
    }
}
