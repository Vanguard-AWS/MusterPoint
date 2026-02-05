using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using MusterPoint.Client.Models;

namespace MusterPoint.Controllers;

/// <summary>
/// Proxy controller for fetching detachments from BSdata catalogues
/// Parses the XML and extracts detachment information
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BSdataDetachmentController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BSdataDetachmentController> _logger;
    private const string BSDATA_RAW_URL = "https://raw.githubusercontent.com/BSData/wh40k-10e/refs/heads/main";

    public BSdataDetachmentController(HttpClient httpClient, ILogger<BSdataDetachmentController> logger)
{
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get detachments for a faction
    /// GET /api/bsdatadetachment/faction/Space%20Marines
    /// </summary>
    [HttpGet("faction/{factionName}")]
    public async Task<ActionResult<List<Detachment>>> GetDetachmentsByFaction([FromRoute] string factionName)
    {
   try
        {
     if (string.IsNullOrWhiteSpace(factionName))
         {
 _logger.LogWarning("[BSdataDetachment] GetDetachmentsByFaction called with empty faction name");
        return BadRequest("Faction name is required");
      }

      _logger.LogInformation($"[BSdataDetachment] ===== START DETACHMENT FETCH FOR {factionName} =====");

     var detachments = new List<Detachment>();

     // Check if this is a Space Marine successor chapter
    var spaceMarinaSuccessorChapters = new List<string>
    {
     "Blood Angels", "Dark Angels", "Deathwatch", "Imperial Fists", "Iron Hands",
             "Raven Guard", "Salamanders", "Space Wolves", "Ultramarines", "White Scars", "Black Templars"
            };

  if (spaceMarinaSuccessorChapters.Contains(factionName))
{
        _logger.LogInformation($"[BSdataDetachment] {factionName} is a Space Marine successor chapter");
        _logger.LogInformation($"[BSdataDetachment] Loading generic Space Marines detachments first...");

      // Load generic Space Marines detachments
     var spaceMarinesDetachments = await LoadDetachmentsFromCatalogueAsync("Space Marines");
      detachments.AddRange(spaceMarinesDetachments);

  _logger.LogInformation($"[BSdataDetachment] Loaded {spaceMarinesDetachments.Count} generic Space Marines detachments");

         // Load successor chapter specific detachments
      var successorDetachments = await LoadDetachmentsFromCatalogueAsync(factionName);
     detachments.AddRange(successorDetachments);

     _logger.LogInformation($"[BSdataDetachment] Loaded {successorDetachments.Count} {factionName} specific detachments");
   }
        else
    {
     // Regular faction loading
 detachments = await LoadDetachmentsFromCatalogueAsync(factionName);
    }

      // Filter detachments based on chapter-specific mappings
         detachments = FilterDetachmentsByChapter(detachments, factionName);

    _logger.LogInformation($"[BSdataDetachment] ===== RESULT: {detachments.Count} detachments found for {factionName} =====");

      return Ok(detachments);
        }
        catch (Exception ex)
        {
     _logger.LogError($"[BSdataDetachment] ❌ EXCEPTION: {ex.Message}");
          _logger.LogError($"[BSdataDetachment] Stack: {ex.StackTrace}");
       return StatusCode(500, $"Error fetching detachments: {ex.Message}");
        }
 }

    /// <summary>
    /// Filter detachments based on chapter-specific rules
    /// Each chapter has a specific set of detachments it can use
    /// </summary>
    private List<Detachment> FilterDetachmentsByChapter(List<Detachment> allDetachments, string factionName)
    {
  var allowedDetachmentNames = GetAllowedDetachmentsForChapter(factionName);
      
        if (allowedDetachmentNames.Count == 0)
        {
  // No filtering needed - return all detachments
            _logger.LogInformation($"[BSdataDetachment] No chapter-specific filtering for {factionName}, returning all detachments");
            return allDetachments;
        }

      var filtered = allDetachments
  .Where(d => allowedDetachmentNames.Contains(d.Name, StringComparer.OrdinalIgnoreCase))
   .ToList();

        _logger.LogInformation($"[BSdataDetachment] Filtered {allDetachments.Count} detachments to {filtered.Count} for chapter {factionName}");
        
      var removedDetachments = allDetachments
  .Where(d => !allowedDetachmentNames.Contains(d.Name, StringComparer.OrdinalIgnoreCase))
   .Select(d => d.Name)
            .ToList();

  if (removedDetachments.Count > 0)
 {
     _logger.LogInformation($"[BSdataDetachment] Removed detachments: {string.Join(", ", removedDetachments)}");
        }

        return filtered;
    }

    /// <summary>
    /// Get the list of allowed detachment names for a specific chapter
    /// </summary>
    private HashSet<string> GetAllowedDetachmentsForChapter(string chapterName)
    {
        // Base Space Marines detachments available to all chapters
        var baseSpaceMarinesDetachments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
     "1st Company Task Force",
            "Anvil Siege Force",
            "Bastion Task Force",
   "Firestorm Assault Force",
 "Gladius Task Force",
      "Ironstorm Spearhead",
        "Librarius Conclave",
       "Orbital Assault Force",
       "Stormlance Task Force",
       "Vanguard Spearhead"
        };

        // Chapter-specific mappings
        var chapterDetachments = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
      "Space Marines",
     new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
   },
       {
           "Black Templars",
     new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
      {
"Companions of Vehemence",
 "Godhammer Assault Force",
       "Vindication Task Force",
   "Wrathful Procession"
     }
            },
     {
     "Blood Angels",
  new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
       {
             "Angelic Inheritors",
  "Liberator Assault Group",
     "Rage-Cursed Onslaught",
           "The Angelic Host",
      "The Lost Brethren"
                }
            },
            {
          "Dark Angels",
  new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
          {
         "Company of Hunters",
       "Inner Circle Task Force",
    "Lion's Blade Task Force",
      "Unforgiven Task Force",
             "Wrath of the Rock"
                }
         },
            {
       "Deathwatch",
                new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
        {
          "Black Spear Task Force"
                }
            },
   {
              "Imperial Fists",
             new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
 {
      "Emperor's Shield"
          }
    },
            {
     "Iron Hands",
             new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
     {
           "Hammer of Avernii"
         }
    },
            {
           "Raven Guard",
      new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
    {
        "Shadowmark Talon"
 }
            },
    {
    "Salamanders",
    new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
    {
             "Forgefather's Seekers"
    }
          },
            {
 "Space Wolves",
           new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
                {
           "Champions of Fenris",
           "Saga of the Beastslayer",
          "Saga of the Bold",
        "Saga of the Great Wolf",
           "Saga of the Hunter"
           }
      },
     {
          "Ultramarines",
new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
                {
    "Blade of Ultramar",
          "Reclamation Force"
            }
        },
       {
      "White Scars",
 new HashSet<string>(baseSpaceMarinesDetachments, StringComparer.OrdinalIgnoreCase)
 {
      "Spearpoint Task Force"
        }
         }
        };

     if (chapterDetachments.TryGetValue(chapterName, out var allowed))
        {
  return allowed;
        }

 // Return empty set for unknown chapters (will not filter)
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
 
    /// <summary>
    /// Load detachments from a faction's catalogue (main + library fallback)
    /// </summary>
    private async Task<List<Detachment>> LoadDetachmentsFromCatalogueAsync(string factionName)
    {
 var cataloguePath = MapFactionToPath(factionName);
        var catalogueUrl = $"{BSDATA_RAW_URL}/{Uri.EscapeDataString(cataloguePath)}";

        _logger.LogInformation($"[BSdataDetachment] Mapped faction '{factionName}' to path: {cataloguePath}");
        _logger.LogInformation($"[BSdataDetachment] Full URL: {catalogueUrl}");

        var response = await _httpClient.GetAsync(catalogueUrl);

        if (!response.IsSuccessStatusCode)
        {
     _logger.LogWarning($"[BSdataDetachment] ❌ Failed to fetch main catalogue: HTTP {response.StatusCode}");
 return new List<Detachment>();
        }

 _logger.LogInformation($"[BSdataDetachment] ✓ HTTP 200 OK - Main catalogue retrieved");

        var xmlContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation($"[BSdataDetachment] Received XML: {xmlContent.Length} bytes");

        var detachments = ParseDetachmentsFromCatalogue(xmlContent, factionName);

 // If no detachments found in main catalogue, try the Library version
        if (detachments.Count == 0)
        {
       _logger.LogInformation($"[BSdataDetachment] No detachments in main catalogue, trying Library variant...");

        var libraryPath = MapFactionToLibraryPath(factionName);
   if (!string.IsNullOrEmpty(libraryPath) && libraryPath != cataloguePath)
          {
      var libraryUrl = $"{BSDATA_RAW_URL}/{Uri.EscapeDataString(libraryPath)}";
         _logger.LogInformation($"[BSdataDetachment] Trying Library catalogue: {libraryUrl}");

              var libraryResponse = await _httpClient.GetAsync(libraryUrl);
          if (libraryResponse.IsSuccessStatusCode)
                {
        _logger.LogInformation($"[BSdataDetachment] ✓ Library catalogue found");
         var libraryXmlContent = await libraryResponse.Content.ReadAsStringAsync();
   _logger.LogInformation($"[BSdataDetachment] Library XML: {libraryXmlContent.Length} bytes");

         detachments = ParseDetachmentsFromCatalogue(libraryXmlContent, factionName);
   _logger.LogInformation($"[BSdataDetachment] Library parse result: {detachments.Count} detachments");
       }
                else
           {
    _logger.LogWarning($"[BSdataDetachment] ❌ Library catalogue not found (HTTP {libraryResponse.StatusCode})");
  _logger.LogInformation($"[BSdataDetachment] This faction may not have detachments in a separate Library file");
             }
         }
        }

        return detachments;
    }
    /// <summary>
    /// Parse detachments from BattleScribe XML catalogue
    /// Detachments can be in different locations depending on the faction
    /// </summary>
    private List<Detachment> ParseDetachmentsFromCatalogue(string xmlContent, string faction)
  {
        var detachments = new List<Detachment>();

      try
        {
      _logger.LogInformation($"[BSdataDetachment Parse] Parsing XML for {faction}");

     var doc = XDocument.Parse(xmlContent);
     var ns = doc.Root?.Name.NamespaceName;
   var xns = ns != null ? XNamespace.Get(ns) : null;

            _logger.LogInformation($"[BSdataDetachment Parse] XML parsed successfully. Namespace: {ns}");
        _logger.LogInformation($"[BSdataDetachment Parse] Root element: {doc.Root?.Name.LocalName}");

            var detachmentEntries = new List<XElement>();

            // Helper function to get elements with or without namespace
      Func<XElement, string, IEnumerable<XElement>> GetElementsByName = (parent, localName) =>
     {
        // Try with namespace first
        var results = parent?.Elements(xns + localName).ToList() ?? new List<XElement>();
         // If nothing found and namespace exists, try without namespace
      if (results.Count == 0 && xns != null)
    {
     results = parent?.Elements(XName.Get(localName)).ToList() ?? new List<XElement>();
         if (results.Count > 0)
                {
            _logger.LogInformation($"[BSdataDetachment Parse] Found elements without namespace: {localName}");
            }
   }
      return results;
            };

      // Location 1: sharedSelectionEntryGroups > selectionEntryGroup[@name='Detachment' or @name='Detachments']
      var sharedGroupsWithNs = doc.Descendants(xns + "sharedSelectionEntryGroups").FirstOrDefault();
  var sharedGroupsNoNs = doc.Descendants(XName.Get("sharedSelectionEntryGroups")).FirstOrDefault();
     var sharedGroups = sharedGroupsWithNs ?? sharedGroupsNoNs;

   if (sharedGroups != null)
            {
   _logger.LogInformation($"[BSdataDetachment Parse] ✓ Found sharedSelectionEntryGroups element");

     var detachmentGroupsList = GetElementsByName(sharedGroups, "selectionEntryGroup")
   .Where(g => 
   {
     var name = g.Attribute("name")?.Value ?? "";
return name == "Detachment" || name == "Detachments";
   })
          .ToList();

   if (detachmentGroupsList.Count > 0)
         {
 var detachmentGroup = detachmentGroupsList.First();
        _logger.LogInformation($"[BSdataDetachment Parse] ✓ Found 'Detachment(s)' selectionEntryGroup in sharedSelectionEntryGroups");

           var selectionEntries = GetElementsByName(detachmentGroup, "selectionEntries").FirstOrDefault();
        if (selectionEntries != null)
{
  detachmentEntries = GetElementsByName(selectionEntries, "selectionEntry").ToList();
    _logger.LogInformation($"[BSdataDetachment Parse] Found {detachmentEntries.Count} selectionEntry elements in sharedSelectionEntryGroups");
   }
     }
        else
    {
  _logger.LogInformation($"[BSdataDetachment Parse] ⚠️ No 'Detachment' or 'Detachments' group in sharedSelectionEntryGroups");
           }
         }
    else
         {
 _logger.LogInformation($"[BSdataDetachment Parse] ⚠️ No sharedSelectionEntryGroups found");
            }

 // Location 2: Look for selectionEntryGroup anywhere in the document with name='Detachment' or 'Detachments'
            if (detachmentEntries.Count == 0)
            {
      _logger.LogInformation($"[BSdataDetachment Parse] Searching for ANY 'Detachment' or 'Detachments' selectionEntryGroup in document");

       var allDetachmentGroupsWithNs = doc.Descendants(xns + "selectionEntryGroup")
   .Where(g => 
   {
     var name = g.Attribute("name")?.Value ?? "";
     return name == "Detachment" || name == "Detachments";
   })
 .ToList();

      var allDetachmentGroupsNoNs = doc.Descendants(XName.Get("selectionEntryGroup"))
    .Where(g => 
    {
      var name = g.Attribute("name")?.Value ?? "";
      return name == "Detachment" || name == "Detachments";
    })
   .ToList();

       var allDetachmentGroups = allDetachmentGroupsWithNs.Count > 0 ? allDetachmentGroupsWithNs : allDetachmentGroupsNoNs;

         _logger.LogInformation($"[BSdataDetachment Parse] Found {allDetachmentGroups.Count} 'Detachment(s)' groups total");

        if (allDetachmentGroups.Count > 0)
       {
   var detachmentGroup = allDetachmentGroups.First();
      _logger.LogInformation($"[BSdataDetachment Parse] ✓ Found 'Detachment(s)' selectionEntryGroup (using first match)");

  // Try selectionEntries first
      var selectionEntries = GetElementsByName(detachmentGroup, "selectionEntries").FirstOrDefault();
  if (selectionEntries != null)
        {
      detachmentEntries = GetElementsByName(selectionEntries, "selectionEntry").ToList();
     _logger.LogInformation($"[BSdataDetachment Parse] Found {detachmentEntries.Count} selectionEntry elements via selectionEntries container");
     }
   else
         {
 // Try direct selectionEntry elements
       detachmentEntries = GetElementsByName(detachmentGroup, "selectionEntry").ToList();
     _logger.LogInformation($"[BSdataDetachment Parse] Found {detachmentEntries.Count} direct selectionEntry elements");
        }
       }
    else
            {
       _logger.LogWarning($"[BSdataDetachment Parse] ❌ No 'Detachment' or 'Detachments' selectionEntryGroup found anywhere in document!");

    // Log what we DO find at root level
     var rootSelectionEntryGroupsWithNs = doc.Root?.Elements(xns + "selectionEntryGroup")
  .Select(g => $"{g.Attribute("name")?.Value ?? "[NO NAME]"} (type: {g.Attribute("type")?.Value ?? "none"})")
         .Take(20)
         .ToList() ?? new List<string>();

     var rootSelectionEntryGroupsNoNs = doc.Root?.Elements(XName.Get("selectionEntryGroup"))
           .Select(g => $"{g.Attribute("name")?.Value ?? "[NO NAME]"} (type: {g.Attribute("type")?.Value ?? "none"})")
           .Take(20)
     .ToList() ?? new List<string>();

 var rootGroups = rootSelectionEntryGroupsWithNs.Count > 0 ? rootSelectionEntryGroupsWithNs : rootSelectionEntryGroupsNoNs;
          
   if (rootGroups.Count > 0)
{
            _logger.LogInformation($"[BSdataDetachment Parse] Root-level selectionEntryGroups: {string.Join(" | ", rootGroups)}");
}
      else
         {
        _logger.LogInformation($"[BSdataDetachment Parse] No root-level selectionEntryGroups found");
        }

            // Also check for any other structure that might contain detachments
    var allGroups = doc.Descendants(XName.Get("selectionEntryGroup"))
          .Select(g => g.Attribute("name")?.Value ?? "[NO NAME]")
                 .Distinct()
       .OrderBy(n => n)
    .Take(30)
          .ToList();

    _logger.LogInformation($"[BSdataDetachment Parse] ALL selectionEntryGroup names in document: {string.Join(", ", allGroups)}");
                }
  }

 _logger.LogInformation($"[BSdataDetachment Parse] Total detachment entries to process: {detachmentEntries.Count}");

        // Parse the detachments we found
      foreach (var entry in detachmentEntries)
   {
       try
    {
   var name = entry.Attribute("name")?.Value;
     var id = entry.Attribute("id")?.Value ?? Guid.NewGuid().ToString();

           if (string.IsNullOrEmpty(name))
continue;

          _logger.LogInformation($"[BSdataDetachment Parse] ✓ Processing detachment: '{name}'");

   var requiredSubfactionIds = ExtractRequiredSubfactions(entry, xns);

            var detachmentRule = new DetachmentRule
          {
Name = name,
      Description = GetDetachmentDescription(entry, xns)
                 };

          var detachment = new Detachment
          {
    Id = id,
            Name = name,
     Faction = faction,
     FactionKeyword = faction,
                 DetachmentRule = detachmentRule,
         Enhancements = new(),
  RequiredSubfactionIds = requiredSubfactionIds
    };

 detachments.Add(detachment);

         if (requiredSubfactionIds.Count > 0)
        {
         _logger.LogInformation($"[BSdataDetachment Parse] ✓ Added detachment: {name} (requires subfactions: {string.Join(", ", requiredSubfactionIds)})");
 }
          else
            {
     _logger.LogInformation($"[BSdataDetachment Parse] ✓ Added detachment: {name} (available to all subfactions)");
 }
          }
    catch (Exception ex)
            {
         _logger.LogWarning($"[BSdataDetachment Parse] Error processing detachment entry: {ex.Message}");
  }
      }
   }
 catch (Exception ex)
        {
_logger.LogError($"[BSdataDetachment Parse] ❌ PARSE ERROR: {ex.Message}");
      _logger.LogError($"[BSdataDetachment Parse] Stack: {ex.StackTrace}");
        }

     return detachments;
    }

    /// <summary>
    /// Try to find the description for a detachment from its rules
    /// </summary>
 private string GetDetachmentDescription(XElement entry, XNamespace? ns)
    {
        try
        {
     ns ??= entry.Name.NamespaceName;

    // Look for rules element
            var rulesElement = entry.Element(ns + "rules");
     if (rulesElement != null)
      {
      var rule = rulesElement.Element(ns + "rule");
         if (rule != null)
  {
         var ruleDescription = rule.Element(ns + "description")?.Value;
         if (!string.IsNullOrEmpty(ruleDescription))
          {
     _logger.LogInformation($"[BSdataDetachment] Found rule description");
        return ruleDescription;
        }
  }
            }
    }
        catch (Exception ex)
        {
            _logger.LogWarning($"[BSdataDetachment] Error getting detachment description: {ex.Message}");
  }

        return "Detachment";
    }

    /// <summary>
  /// Extract subfaction IDs that this detachment requires
    /// Detachments have modifiers with conditions like:
    /// "notInstanceOf" or "instanceOf" with childId matching subfaction selections
/// </summary>
    private List<string> ExtractRequiredSubfactions(XElement entry, XNamespace? ns)
    {
        var requiredIds = new List<string>();

try
        {
  ns ??= entry.Name.NamespaceName;

     // Look for modifiers that control visibility
    var modifiers = entry.Descendants(ns + "modifier").ToList();

 foreach (var modifier in modifiers)
         {
 // We're looking for modifiers that set "hidden" to true with conditions
  var field = modifier.Attribute("field")?.Value;
       if (field != "hidden")
     continue;

            var conditions = modifier.Elements(ns + "condition").ToList();
           
 foreach (var condition in conditions)
  {
     var conditionType = condition.Attribute("type")?.Value;
        var childId = condition.Attribute("childId")?.Value;
      
    // "instanceOf" means: show ONLY if this subfaction is selected
      // "notInstanceOf" means: hide if this subfaction is NOT selected (i.e., show only for other subfactions)
      if (!string.IsNullOrEmpty(childId) && (conditionType == "instanceOf" || conditionType == "notInstanceOf"))
        {
    if (conditionType == "instanceOf")
        {
          // This detachment requires this subfaction
        requiredIds.Add(childId);
    _logger.LogInformation($"[BSdataDetachment Parse]   → Requires subfaction: {childId}");
   }
      }
        }
  }
        }
        catch (Exception ex)
    {
_logger.LogWarning($"[BSdataDetachment Parse] Error extracting subfaction requirements: {ex.Message}");
        }

        return requiredIds;
    }

 /// <summary>
    /// Maps faction names to BSdata catalogue paths
 /// </summary>
  private string MapFactionToPath(string factionName)
    {
 var pathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
  // Imperium factions (19 total)
   { "Space Marines", "Imperium - Space Marines.cat" },
    { "Blood Angels", "Imperium - Blood Angels.cat" },
            { "Dark Angels", "Imperium - Dark Angels.cat" },
     { "Deathwatch", "Imperium - Deathwatch.cat" },
   { "Imperial Fists", "Imperium - Imperial Fists.cat" },
    { "Iron Hands", "Imperium - Iron Hands.cat" },
   { "Raven Guard", "Imperium - Raven Guard.cat" },
    { "Salamanders", "Imperium - Salamanders.cat" },
   { "Space Wolves", "Imperium - Space Wolves.cat" },
    { "Ultramarines", "Imperium - Ultramarines.cat" },
   { "White Scars", "Imperium - White Scars.cat" },
 { "Imperial Knights", "Imperium - Imperial Knights.cat" },
   { "Adeptus Mechanicus", "Imperium - Adeptus Mechanicus.cat" },
      { "Grey Knights", "Imperium - Grey Knights.cat" },
       { "Black Templars", "Imperium - Black Templars.cat" },
            { "Astra Militarum", "Imperium - Astra Militarum.cat" },
            { "Adepta Sororitas", "Imperium - Adepta Sororitas.cat" },
            { "Adeptus Custodes", "Imperium - Adeptus Custodes.cat" },
     { "Agents of the Imperium", "Imperium - Agents of the Imperium.cat" },

         // Chaos factions (7 total)
{ "Chaos Space Marines", "Chaos - Chaos Space Marines.cat" },
            { "Chaos Daemons", "Chaos - Chaos Daemons.cat" },
       { "Death Guard", "Chaos - Death Guard.cat" },
    { "Thousand Sons", "Chaos - Thousand Sons.cat" },
      { "Chaos Knights", "Chaos - Chaos Knights.cat" },
      { "Emperor's Children", "Chaos - Emperor's Children.cat" },
            { "World Eaters", "Chaos - World Eaters.cat" },

       // Xenos factions
    { "Necrons", "Necrons.cat" },
            { "Orks", "Orks.cat" },
  { "T'au Empire", "T'au Empire.cat" },
     { "Tyranids", "Tyranids.cat" },
  { "Leagues of Votann", "Leagues of Votann.cat" },
            { "Aeldari", "Aeldari - Craftworlds.cat" },
 { "Drukhari", "Aeldari - Drukhari.cat" },
   };

if (pathMap.TryGetValue(factionName, out var path))
  return path;

  // Fallback
        return $"{factionName}.cat";
  }

    /// <summary>
    /// Maps faction names to BSdata catalogue Library paths (if they exist)
    /// Some factions have separate Library files with different naming conventions
    /// </summary>
    private string MapFactionToLibraryPath(string factionName)
    {
        // Explicit mappings for factions with Library files
      var libraryPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
     // Xenos - "Library - <Name>.cat" pattern
     { "Tyranids", "Library - Tyranids.cat" },
  { "Necrons", "Library - Necrons.cat" },
{ "Orks", "Library - Orks.cat" },
            { "T'au Empire", "Library - T'au Empire.cat" },
 { "Leagues of Votann", "Library - Leagues of Votann.cat" },
         { "Genestealer Cults", "Library - Genestealer Cults.cat" },

       // Xenos - Aeldari uses special library
         { "Aeldari", "Aeldari - Aeldari Library.cat" },
 { "Drukhari", "Aeldari - Aeldari Library.cat" },

    // Chaos - "<Faction> - <Faction> Library.cat" pattern
    { "Chaos Daemons", "Chaos - Chaos Daemons Library.cat" },
  { "Chaos Knights", "Chaos - Chaos Knights Library.cat" },
  { "Chaos Space Marines", "Chaos - Chaos Space Marines Library.cat" },
{ "Death Guard", "Chaos - Death Guard Library.cat" },
    { "Emperor's Children", "Chaos - Emperor's Children Library.cat" },
{ "Thousand Sons", "Chaos - Thousand Sons Library.cat" },
  { "World Eaters", "Chaos - World Eaters Library.cat" },

         // Imperium - "<Faction> - <Faction> Library.cat" pattern
          { "Adeptus Mechanicus", "Imperium - Adeptus Mechanicus Library.cat" },
   { "Grey Knights", "Imperium - Grey Knights Library.cat" },
       { "Adeptus Custodes", "Imperium - Adeptus Custodes Library.cat" },
  { "Space Marines", "Imperium - Space Marines Library.cat" },
  { "Blood Angels", "Imperium - Blood Angels Library.cat" },
   { "Dark Angels", "Imperium - Dark Angels Library.cat" },
 { "Deathwatch", "Imperium - Deathwatch Library.cat" },
      { "Imperial Fists", "Imperium - Imperial Fists Library.cat" },
{ "Iron Hands", "Imperium - Iron Hands Library.cat" },
 { "Raven Guard", "Imperium - Raven Guard Library.cat" },
 { "Salamanders", "Imperium - Salamanders Library.cat" },
      { "Space Wolves", "Imperium - Space Wolves Library.cat" },
   { "Ultramarines", "Imperium - Ultramarines Library.cat" },
{ "White Scars", "Imperium - White Scars Library.cat" },
    { "Imperial Knights", "Imperium - Imperial Knights - Library.cat" },
  { "Black Templars", "Imperium - Black Templars Library.cat" },
        { "Astra Militarum", "Imperium - Astra Militarum - Library.cat" },
         { "Adepta Sororitas", "Imperium - Adepta Sororitas Library.cat" },
    { "Agents of the Imperium", "Imperium - Agents of the Imperium Library.cat" },
      };

        if (libraryPathMap.TryGetValue(factionName, out var path))
        {
  return path;
        }

        // Default fallback pattern (shouldn't be needed with explicit mappings)
        return $"{factionName} - {factionName} Library.cat";
    }
}
