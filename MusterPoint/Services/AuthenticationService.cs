using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MusterPoint.Models.Authentication;

namespace MusterPoint.Services;

/// <summary>
/// Handles JWT token generation and validation for authentication
/// </summary>
public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
 /// Generates a JWT token for an authenticated user
    /// </summary>
    public string GenerateToken(UserDto user)
    {
        try
    {
      var jwtSettings = _configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "0");

            if (string.IsNullOrEmpty(secretKey))
       throw new InvalidOperationException("JWT SecretKey is not configured");

 var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

         var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
};

            // Set expires to null if ExpirationMinutes is 0 (no expiration)
      DateTime? expires = expirationMinutes > 0 
     ? DateTime.UtcNow.AddMinutes(expirationMinutes)
       : null;

   var token = new JwtSecurityToken(
        issuer: issuer,
      audience: audience,
                claims: claims,
          expires: expires,
          signingCredentials: creds
     );

   return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
       _logger.LogError($"Error generating JWT token: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Validates a JWT token and extracts claims
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
      try
        {
  var jwtSettings = _configuration.GetSection("JwtSettings");
         var secretKey = jwtSettings["SecretKey"];
         var issuer = jwtSettings["Issuer"];
     var audience = jwtSettings["Audience"];

          var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

            var tokenHandler = new JwtSecurityTokenHandler();
 var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
          IssuerSigningKey = key,
      ValidateIssuer = true,
                ValidIssuer = issuer,
        ValidateAudience = true,
           ValidAudience = audience,
         ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
         }, out SecurityToken validatedToken);

         return principal;
}
        catch (Exception ex)
        {
    _logger.LogError($"Token validation failed: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// Handles password hashing and verification
/// </summary>
public class PasswordHashService
{
    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

/// <summary>
/// Main authentication service for login/register operations
/// </summary>
public class AuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly PasswordHashService _passwordHashService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        PasswordHashService passwordHashService,
        JwtTokenService jwtTokenService)
    {
        _logger = logger;
  _passwordHashService = passwordHashService;
      _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Authenticates a user with username and password
  /// </summary>
    public async Task<AuthResponse> AuthenticateAsync(LoginRequest request, User? user)
    {
        if (user == null)
   {
  return new AuthResponse
  {
           Success = false,
              Message = "Invalid username or password"
 };
        }

 if (!user.IsActive)
        {
    return new AuthResponse
            {
       Success = false,
    Message = "User account is inactive"
            };
        }

        if (!_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
 {
            return new AuthResponse
{
                Success = false,
            Message = "Invalid username or password"
     };
        }

        var userDto = new UserDto
        {
Id = user.Id,
            Username = user.Username,
        Email = user.Email
      };

        var token = _jwtTokenService.GenerateToken(userDto);

        return new AuthResponse
        {
            Success = true,
      Message = "Login successful",
            Token = token,
   User = userDto
        };
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation($"RegisterAsync called for username: {request.Username}");
      
  if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning($"Registration failed for '{request.Username}': Passwords do not match");
       return new AuthResponse
  {
    Success = false,
     Message = "Passwords do not match"
            };
  }

        if (request.Password.Length < 8)
        {
            _logger.LogWarning($"Registration failed for '{request.Username}': Password too short");
        return new AuthResponse
            {
  Success = false,
        Message = "Password must be at least 8 characters long"
          };
        }

        _logger.LogInformation($"Validation passed for username: {request.Username}");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
  PasswordHash = "[will be set in controller]"
        };

        return new AuthResponse
        {
         Success = true,
       Message = "Validation successful. User can now be created."
        };
    }
}
