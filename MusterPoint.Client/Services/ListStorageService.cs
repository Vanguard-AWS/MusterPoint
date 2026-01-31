using MusterPoint.Client.Models;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace MusterPoint.Client.Services;

/// <summary>
/// Client-side list storage service that communicates with the API
/// </summary>
public class ListStorageService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ListStorageService> _logger;
    private readonly AuthService _authService;

    public ListStorageService(HttpClient httpClient, ILogger<ListStorageService> logger, AuthService authService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _authService = authService;
    }

    private async Task<bool> EnsureAuthTokenAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No authentication token available - user may not be authenticated");
                return false;
            }

            _logger.LogInformation($"Token retrieved successfully, length: {token.Length}");

            // Set the Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error ensuring authentication token: {ex.Message}");
            return false;
        }
    }

    public async Task<List<UserList>> GetAllListsAsync()
    {
        try
        {
            if (!await EnsureAuthTokenAsync())
                return new();

            var response = await _httpClient.GetAsync("api/lists");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to get lists: {response.StatusCode}");
                return new();
            }

            var lists = await response.Content.ReadFromJsonAsync<List<UserList>>();
            return lists ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading lists from server: {ex.Message}");
            return new();
        }
    }

    public async Task<UserList?> GetListByIdAsync(string listId)
    {
        try
        {
            if (!await EnsureAuthTokenAsync())
                return null;

            var response = await _httpClient.GetAsync($"api/lists/{listId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserList>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading list from server: {ex.Message}");
            return null;
        }
    }

    public async Task SaveListAsync(UserList list)
    {
        try
        {
            if (!await EnsureAuthTokenAsync())
            {
                _logger.LogError("Failed to ensure authentication - no token available");
                throw new InvalidOperationException("Not authenticated");
            }

            _logger.LogInformation($"Saving list '{list.Name}' with {list.Entries.Count} entries");
            
            var response = await _httpClient.PostAsJsonAsync("api/lists", list);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error saving list: {response.StatusCode} - {errorContent}");
                throw new InvalidOperationException($"Failed to save list: {response.StatusCode}");
            }

            _logger.LogInformation($"List '{list.Name}' saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving list to server: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteListAsync(string listId)
    {
        try
        {
            if (!await EnsureAuthTokenAsync())
                throw new InvalidOperationException("Not authenticated");

            var response = await _httpClient.DeleteAsync($"api/lists/{listId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error deleting list: {response.StatusCode}");
                throw new InvalidOperationException("Failed to delete list");
            }

            _logger.LogInformation($"List '{listId}' deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting list from server: {ex.Message}");
            throw;
        }
    }

    public async Task<string> ExportListAsJsonAsync(UserList list)
    {
        return System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<UserList?> ImportListFromJsonAsync(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<UserList>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error importing list: {ex.Message}");
            return null;
        }
    }
}
