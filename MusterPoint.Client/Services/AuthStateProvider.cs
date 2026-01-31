namespace MusterPoint.Client.Services;

/// <summary>
/// Provides authentication state management using localStorage
/// Restores authentication from localStorage on initialization
/// </summary>
public class AuthStateProvider
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthStateProvider> _logger;
    private Action? _notifyStateChangedAction;

    public AuthStateProvider(AuthService authService, ILogger<AuthStateProvider> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Set the callback for state changes
    /// </summary>
    public void SetNotifyStateChangedAction(Action notifyStateChanged)
    {
        _notifyStateChangedAction = notifyStateChanged;
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
   try
        {
  var token = await _authService.GetTokenAsync();
          return !string.IsNullOrEmpty(token);
 }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking authentication: {ex.Message}");
          return false;
        }
    }

    /// <summary>
    /// Notify that the user has logged in
    /// </summary>
    public void NotifyUserLogin()
    {
        _notifyStateChangedAction?.Invoke();
        _logger.LogInformation("User login notification sent");
    }

    /// <summary>
    /// Notify that the user has logged out
    /// </summary>
    public void NotifyUserLogout()
    {
        _notifyStateChangedAction?.Invoke();
        _logger.LogInformation("User logout notification sent");
    }
}
