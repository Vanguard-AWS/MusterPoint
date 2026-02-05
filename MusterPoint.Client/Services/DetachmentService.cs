using System.Net.Http.Json;
using MusterPoint.Client.Models;

namespace MusterPoint.Client.Services;

public class DetachmentService
{
    private readonly HttpClient _httpClient;
    private Dictionary<string, List<Detachment>> _detachmentCache = new();

    public DetachmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Detachment>> GetDetachmentsByFactionAsync(string faction)
    {
        try
        {
            Console.WriteLine($"[DetachmentService] Loading detachments for faction: {faction}");

            // Check cache first
            if (_detachmentCache.TryGetValue(faction, out var cached))
            {
                Console.WriteLine($"[DetachmentService] ? Returning cached detachments for {faction} ({cached.Count} found)");
                return cached;
            }

            // Load from BSdata via backend proxy
            Console.WriteLine($"[DetachmentService] ?? Fetching detachments from BSdata endpoint");
            var detachments = await LoadDetachmentsFromBSdataAsync(faction);

            // Cache the result
            _detachmentCache[faction] = detachments;
            Console.WriteLine($"[DetachmentService] ? Cached {detachments.Count} detachments for {faction}");

            return detachments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DetachmentService] ? Error loading detachments for {faction}: {ex.Message}");
            Console.WriteLine($"[DetachmentService] Stack: {ex.StackTrace}");
            return new();
        }
    }

    /// <summary>
    /// Force refresh detachments for a faction by clearing cache and reloading
    /// </summary>
    public async Task<List<Detachment>> RefreshDetachmentsAsync(string faction)
    {
        Console.WriteLine($"[DetachmentService] ?? Force refreshing detachments for {faction}");
        _detachmentCache.Remove(faction);
        return await GetDetachmentsByFactionAsync(faction);
    }

    /// <summary>
    /// Clear the entire detachment cache
    /// </summary>
    public void ClearCache()
    {
        Console.WriteLine($"[DetachmentService] ???  Clearing detachment cache ({_detachmentCache.Count} entries)");
        _detachmentCache.Clear();
    }

    /// <summary>
    /// Load detachments from BSdata via backend proxy
    /// </summary>
    private async Task<List<Detachment>> LoadDetachmentsFromBSdataAsync(string faction)
    {
        try
        {
            var url = $"/api/bsdatadetachment/faction/{Uri.EscapeDataString(faction)}";

            Console.WriteLine($"[DetachmentService] Calling endpoint: {url}");
            var response = await _httpClient.GetAsync(url);

            Console.WriteLine($"[DetachmentService] Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DetachmentService] ? Endpoint returned {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DetachmentService] Error content: {errorContent}");
                return new();
            }

            var detachments = await response.Content.ReadFromJsonAsync<List<Detachment>>();

            if (detachments != null && detachments.Count > 0)
            {
                Console.WriteLine($"[DetachmentService] ? BSdata returned {detachments.Count} detachments");
                foreach (var d in detachments.Take(5))
                {
                    Console.WriteLine($"[DetachmentService]   - {d.Name} (ID: {d.Id})");
                }
                return detachments;
            }

            Console.WriteLine($"[DetachmentService] ?? BSdata returned no detachments");
            return new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DetachmentService] ? Exception calling endpoint: {ex.Message}");
            Console.WriteLine($"[DetachmentService] Stack: {ex.StackTrace}");
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
            Console.WriteLine($"[DetachmentService] Error loading detachment: {ex.Message}");
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
