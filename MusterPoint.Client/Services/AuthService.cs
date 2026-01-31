using System.Net.Http.Json;
using MusterPoint.Client.Models.Authentication;
using Microsoft.JSInterop;

namespace MusterPoint.Client.Services;

/// <summary>
/// Client-side authentication service for login/register operations
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private readonly IJSRuntime _jsRuntime;
    private AuthStateProvider? _authStateProvider;
    private const string StorageKey = "auth_token";
    private const int RequestTimeoutSeconds = 30;

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsRuntime = jsRuntime;
        
        // Ensure timeout is set
        if (_httpClient.Timeout == TimeSpan.Zero || _httpClient.Timeout == Timeout.InfiniteTimeSpan)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);
        }
    }

    /// <summary>
    /// Set the auth state provider (called during initialization)
    /// </summary>
    public void SetAuthStateProvider(AuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest { Username = username, Password = password };
         
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(RequestTimeoutSeconds));
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Login failed: {response.StatusCode} - {errorContent}");
                return new AuthResponse { Success = false, Message = "Login failed" };
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse?.Success == true && !string.IsNullOrEmpty(authResponse.Token))
            {
                // Store token to localStorage
                await SaveTokenAsync(authResponse.Token);
                // Notify auth state provider
                _authStateProvider?.NotifyUserLogin();
            }

            return authResponse ?? new AuthResponse { Success = false, Message = "Invalid response" };
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Login request timed out after 30 seconds");
            return new AuthResponse { Success = false, Message = "Login request timed out. Please check your connection." };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Login network error: {ex.Message}");
            return new AuthResponse { Success = false, Message = "Network error. Please check your connection." };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return new AuthResponse { Success = false, Message = "An error occurred during login" };
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(string username, string email, string password, string confirmPassword)
    {
        try
        {
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(RequestTimeoutSeconds));
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Registration failed: {response.StatusCode} - {errorContent}");
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                return authResponse ?? new AuthResponse { Success = false, Message = $"Registration failed: {response.StatusCode}" };
            }

            var successResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return successResponse ?? new AuthResponse { Success = false, Message = "Invalid response" };
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Registration request timed out after 30 seconds");
            return new AuthResponse { Success = false, Message = "Registration request timed out. Please check your connection." };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Registration network error: {ex.Message}");
            return new AuthResponse { Success = false, Message = "Network error. Please check your connection." };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return new AuthResponse { Success = false, Message = "An error occurred during registration" };
        }
    }

    /// <summary>
    /// Logout (clear token)
    /// </summary>
    public async Task LogoutAsync()
    {
        await ClearTokenAsync();
        // Notify auth state provider
        _authStateProvider?.NotifyUserLogout();
    }

    /// <summary>
    /// Get the stored JWT token from localStorage
    /// </summary>
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token is empty or null in localStorage");
                return null;
            }
            
            _logger.LogInformation($"Token retrieved from localStorage, length: {token.Length}");
            return token;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop") || ex.Message.Contains("statically rendered"))
        {
            // This happens during static rendering/prerendering - token will be fetched after interactive render
            _logger.LogWarning($"Cannot access localStorage during prerendering, will retry after render");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error retrieving token from localStorage: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    /// <summary>
    /// Save JWT token to localStorage
    /// </summary>
    private async Task SaveTokenAsync(string token)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, token);
            _logger.LogInformation("Token saved to localStorage");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving token to localStorage: {ex.Message}");
        }
    }

    /// <summary>
    /// Clear JWT token from localStorage
    /// </summary>
    private async Task ClearTokenAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            _logger.LogInformation("Token cleared from localStorage");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error clearing token from localStorage: {ex.Message}");
        }
    }
}
