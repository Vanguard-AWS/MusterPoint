using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace MusterPoint.Controllers;

/// <summary>
/// Proxy controller for fetching BSdata catalogues from GitHub
/// This avoids CORS issues when fetching from raw.githubusercontent.com
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BSdataProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BSdataProxyController> _logger;
private const string BSDATA_RAW_URL = "https://raw.githubusercontent.com/BSData/wh40k/master";

    public BSdataProxyController(HttpClient httpClient, ILogger<BSdataProxyController> logger)
    {
   _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Fetch a catalogue XML file from BSdata
    /// GET /api/bsdataproxy/catalogue?faction=Space%20Marines
    /// </summary>
    [HttpGet("catalogue")]
    public async Task<IActionResult> GetCatalogue([FromQuery] string faction)
    {
        try
        {
          if (string.IsNullOrWhiteSpace(faction))
          {
 _logger.LogWarning("[BSdataProxy] GetCatalogue called with empty faction");
     return BadRequest("Faction name is required");
            }

            _logger.LogInformation($"[BSdataProxy] Fetching catalogue for faction: {faction}");

   var factionPath = MapFactionToPath(faction);
            var catalogueUrl = $"{BSDATA_RAW_URL}/{factionPath}";

            _logger.LogInformation($"[BSdataProxy] Fetching from URL: {catalogueUrl}");

   var response = await _httpClient.GetAsync(catalogueUrl);

            if (!response.IsSuccessStatusCode)
{
        _logger.LogWarning($"[BSdataProxy] Failed to fetch catalogue for {faction}: HTTP {response.StatusCode}");
    return StatusCode((int)response.StatusCode, $"Failed to fetch catalogue from BSdata: {response.StatusCode}");
  }

          var xmlContent = await response.Content.ReadAsStringAsync();
     _logger.LogInformation($"[BSdataProxy] Successfully fetched {xmlContent.Length} bytes for {faction}");

    // Validate it's valid XML
   try
   {
          XDocument.Parse(xmlContent);
            }
            catch (Exception ex)
            {
           _logger.LogError($"[BSdataProxy] Invalid XML received for {faction}: {ex.Message}");
          return BadRequest("Invalid XML received from BSdata");
   }

          // Return the XML content
     return Ok(new { faction = faction, content = xmlContent });
      }
        catch (Exception ex)
        {
            _logger.LogError($"[BSdataProxy] Error fetching catalogue: {ex.Message}");
        return StatusCode(500, $"Error fetching catalogue: {ex.Message}");
        }
    }

    /// <summary>
    /// Maps faction names to BSdata repository paths
    /// The wh40k-10e repo is at: https://github.com/BSData/wh40k-10e
    /// Files are .cat format at the root level
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
     { "Chaos Daemons", "Chaos - Daemons.cat" },
            { "Death Guard", "Chaos - Death Guard.cat" },
            { "Thousand Sons", "Chaos - Thousand Sons.cat" },
            { "Chaos Knights", "Chaos - Chaos Knights.cat" },
     { "Emperor's Children", "Chaos - Emperor's Children.cat" },
     { "World Eaters", "Chaos - World Eaters.cat" },

    // Xenos factions (6 total)
            { "Necrons", "Necrons.cat" },
    { "Orks", "Orks.cat" },
      { "T'au Empire", "T'au Empire.cat" },
    { "Tyranids", "Tyranids.cat" },
    { "Leagues of Votann", "Leagues of Votann.cat" },
            { "Aeldari", "Aeldari - Aeldari Library.cat" },
        };

        if (pathMap.TryGetValue(factionName, out var path))
 return path;

     // Fallback
        return $"{factionName}.cat";
    }
}
