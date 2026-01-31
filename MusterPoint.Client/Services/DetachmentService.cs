using System.Net.Http.Json;
using MusterPoint.Client.Models;

namespace MusterPoint.Client.Services;

public class DetachmentService
{
    private readonly HttpClient _httpClient;

    public DetachmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Detachment>> GetDetachmentsByFactionAsync(string faction)
    {
  try
        {
      // Load the index file
       var indexResponse = await _httpClient.GetFromJsonAsync<DetachmentIndex>("data/space-marine-detachments/index.json");
            
            if (indexResponse?.Detachments == null)
          return new();

     // Filter by faction and load each detachment
   var detachments = new List<Detachment>();

          foreach (var detachmentMeta in indexResponse.Detachments.Where(d => d.Faction == faction))
   {
                var detachmentResponse = await _httpClient.GetFromJsonAsync<Detachment>(detachmentMeta.Path);
        if (detachmentResponse != null)
   {
       detachments.Add(detachmentResponse);
 }
          }

       return detachments;
}
      catch (Exception ex)
        {
Console.WriteLine($"Error loading detachments: {ex.Message}");
  return new();
        }
    }

    public async Task<Detachment?> GetDetachmentByIdAsync(string faction, string detachmentId)
    {
        try
        {
         var detachments = await GetDetachmentsByFactionAsync(faction);
      return detachments.FirstOrDefault(d => d.Id == detachmentId);
        }
    catch (Exception ex)
    {
      Console.WriteLine($"Error loading detachment: {ex.Message}");
  return null;
    }
    }
}

public class DetachmentIndex
{
    public List<DetachmentMeta> Detachments { get; set; } = new();
}

public class DetachmentMeta
{
    public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
public string Path { get; set; } = string.Empty;
}
