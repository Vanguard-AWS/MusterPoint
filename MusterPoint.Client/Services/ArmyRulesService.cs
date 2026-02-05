using System.Net.Http.Json;
using MusterPoint.Client.Models;

namespace MusterPoint.Client.Services;

public class ArmyRulesService
{
    private readonly HttpClient _httpClient;

    public ArmyRulesService(HttpClient httpClient)
    {
  _httpClient = httpClient;
    }

    public async Task<ArmyRulesData?> GetArmyRulesByFactionAsync(string faction)
    {
        try
        {
            // Convert faction name to kebab-case for file naming
            string kebabFaction = faction.ToLower().Replace(" ", "-").Replace("'", "");
        var response = await _httpClient.GetFromJsonAsync<ArmyRulesData>($"data/army-rules/{kebabFaction}.json");
            return response;
        }
   catch (Exception ex)
        {
       Console.WriteLine($"Error loading army rules: {ex.Message}");
          return null;
    }
    }
}

public class ArmyRulesData
{
    public string Faction { get; set; } = string.Empty;
    public List<ArmyRule> ArmyRules { get; set; } = new();
}

public class ArmyRule
{
  public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FlavorText { get; set; }
    public List<string> Rules { get; set; } = new();
    public List<RuleTable>? Tables { get; set; }
}

public class RuleTable
{
    public string Title { get; set; } = string.Empty;
  public List<RuleTableRow> Rows { get; set; } = new();
}

public class RuleTableRow
{
    public string Roll { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
