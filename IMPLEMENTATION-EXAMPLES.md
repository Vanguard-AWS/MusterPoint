# Implementation Examples - Missing Components

## 1. User Repository Service (Server-Side)

### Create File: `MusterPoint/Services/UserRepository.cs`

```csharp
using MongoDB.Driver;
using MusterPoint.Models.Authentication;

namespace MusterPoint.Services;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int id);
    Task<bool> UserExistsAsync(string username);
    Task CreateUserAsync(User user);
 Task UpdateUserAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;
 private readonly ILogger<UserRepository> _logger;

    public UserRepository(IMongoClient mongoClient, ILogger<UserRepository> logger)
    {
   _logger = logger;
        var database = mongoClient.GetDatabase("musterpoint");
        _usersCollection = database.GetCollection<User>("users");
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
  try
        {
    var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            return await _usersCollection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting user by username: {ex.Message}");
return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
   try
        {
    var filter = Builders<User>.Filter.Eq(u => u.Id, id);
          return await _usersCollection.Find(filter).FirstOrDefaultAsync();
    }
        catch (Exception ex)
        {
     _logger.LogError($"Error getting user by ID: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        try
        {
      var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            var count = await _usersCollection.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (Exception ex)
{
     _logger.LogError($"Error checking user existence: {ex.Message}");
      return false;
        }
    }

    public async Task CreateUserAsync(User user)
    {
        try
        {
 user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
     await _usersCollection.InsertOneAsync(user);
            _logger.LogInformation($"User '{user.Username}' created successfully");
        }
        catch (Exception ex)
     {
    _logger.LogError($"Error creating user: {ex.Message}");
            throw;
   }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
   {
     user.UpdatedAt = DateTime.UtcNow;
  var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var options = new ReplaceOptions { IsUpsert = false };
          await _usersCollection.ReplaceOneAsync(filter, user, options);
            _logger.LogInformation($"User '{user.Username}' updated successfully");
        }
        catch (Exception ex)
        {
_logger.LogError($"Error updating user: {ex.Message}");
        throw;
    }
    }
}
```

### Update `Program.cs` to register the repository:

```csharp
// Add after MongoDB setup
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

### Update `AuthController.cs` to use repository:

```csharp
private readonly IUserRepository _userRepository;

public AuthController(
    AuthenticationService authService, 
    IUserRepository userRepository,
    ILogger<AuthController> logger)
{
    _authService = authService;
    _userRepository = userRepository;
    _logger = logger;
}

[HttpPost("login")]
public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
{
    if (!ModelState.IsValid)
     return BadRequest(ModelState);

    try
    {
  var user = await _userRepository.GetUserByUsernameAsync(request.Username);
  var response = await _authService.AuthenticateAsync(request, user);

        if (!response.Success)
 return Unauthorized(response);

        return Ok(response);
    }
    catch (Exception ex)
    {
     _logger.LogError($"Login error: {ex.Message}");
  return StatusCode(500, new AuthResponse 
{ 
  Success = false, 
            Message = "An error occurred during login" 
        });
    }
}

[HttpPost("register")]
public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
    return BadRequest(ModelState);

    try
    {
        if (await _userRepository.UserExistsAsync(request.Username))
       return BadRequest(new AuthResponse 
            { 
                Success = false, 
   Message = "Username already taken" 
       });

        var response = await _authService.RegisterAsync(request);

        if (!response.Success)
return BadRequest(response);

        // Hash the password and create the user
      var passwordHashService = new PasswordHashService();
        var user = new User
        {
   Username = request.Username,
            Email = request.Email,
        PasswordHash = passwordHashService.HashPassword(request.Password),
     IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    await _userRepository.CreateUserAsync(user);

        return Ok(response);
    }
    catch (Exception ex)
    {
   _logger.LogError($"Registration error: {ex.Message}");
      return StatusCode(500, new AuthResponse 
        { 
          Success = false, 
 Message = "An error occurred during registration" 
        });
    }
}
```

---

## 2. Token Storage in Client (Browser)

### Update `AuthService.cs`:

```csharp
using Microsoft.JSInterop;

public class AuthService
{
    private readonly HttpClient _httpClient;
 private readonly ILogger<AuthService> _logger;
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "auth_token";

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger, IJSRuntime jsRuntime)
    {
  _httpClient = httpClient;
        _logger = logger;
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        try
    {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKey);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving token: {ex.Message}");
            return null;
  }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
     var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    private async Task SaveTokenAsync(string token)
    {
   try
     {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, token);
    _logger.LogInformation("Token saved to localStorage");
        }
    catch (Exception ex)
        {
      _logger.LogError($"Error saving token: {ex.Message}");
        }
    }

    private async Task ClearTokenAsync()
    {
        try
        {
    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            _logger.LogInformation("Token cleared from localStorage");
     }
        catch (Exception ex)
   {
  _logger.LogError($"Error clearing token: {ex.Message}");
      }
    }

    public async Task LogoutAsync()
    {
        await ClearTokenAsync();
    }
}
```

---

## 3. HTTP Handler for Token Injection

### Create File: `MusterPoint.Client/Services/AuthenticationMessageHandler.cs`

```csharp
namespace MusterPoint.Client.Services;

public class AuthenticationMessageHandler : DelegatingHandler
{
    private readonly AuthService _authService;
  private readonly ILogger<AuthenticationMessageHandler> _logger;

    public AuthenticationMessageHandler(AuthService authService, ILogger<AuthenticationMessageHandler> logger)
    {
        _authService = authService;
    _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
 HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var token = await _authService.GetTokenAsync();

 if (!string.IsNullOrEmpty(token))
  {
       request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### Update `Client/Program.cs` to use the handler:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register authentication handler
builder.Services.AddScoped<AuthenticationMessageHandler>();

// Register services
builder.Services.AddScoped(sp => new HttpClient(sp.GetRequiredService<AuthenticationMessageHandler>()) 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DatasheetService>();
builder.Services.AddScoped<ListStorageService>();

await builder.Build().RunAsync();
```

---

## 4. Update User Model for MongoDB

Update `MusterPoint/Models/Authentication/AuthenticationModels.cs`:

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MusterPoint.Models.Authentication;

[BsonIgnoreExtraElements]
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int Id { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}
```

---

## 5. Protected Route Component (Optional)

### Create File: `MusterPoint.Client/Components/AuthorizedOnly.razor`

```razor
@using Microsoft.AspNetCore.Components.Authorization
@using MusterPoint.Client.Services

@if (isAuthenticated)
{
    @ChildContent
}
else
{
    <div class="alert alert-warning">
        You must be logged in to access this content. <a href="/login">Click here to login</a>
    </div>
}

@code {
    [Parameter]
 public RenderFragment? ChildContent { get; set; }

[Inject]
    public AuthService AuthService { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private bool isAuthenticated = false;

    protected override async Task OnInitializedAsync()
    {
        isAuthenticated = await AuthService.IsAuthenticatedAsync();
        if (!isAuthenticated)
        {
   NavigationManager.NavigateTo("/login");
      }
    }
}
```

### Usage in component:

```razor
<AuthorizedOnly>
    <YourProtectedContent />
</AuthorizedOnly>
```

---

## Next Steps

1. **Implement UserRepository** - Follow the code above
2. **Update AuthController** - Use the repository in login/register
3. **Implement token storage** - Use IJSRuntime for localStorage
4. **Add authentication handler** - Automatically inject tokens in requests
5. **Test the flow** - Register ? Login ? Access protected resources

All the core architecture is in place. These implementations just wire everything together!
