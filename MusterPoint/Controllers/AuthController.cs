using Microsoft.AspNetCore.Mvc;
using MusterPoint.Models.Authentication;
using MusterPoint.Services;

namespace MusterPoint.Controllers;

/// <summary>
/// API endpoints for authentication (login, register, token refresh)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;
    private readonly IUserRepository _userRepository;
    private readonly PasswordHashService _passwordHashService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthenticationService authService,
        IUserRepository userRepository,
        PasswordHashService passwordHashService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _logger = logger;
    }

    /// <summary>
    /// Login with username and password
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation($"Login attempt for username: {request.Username}");

            // Get user from database
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);

            var response = await _authService.AuthenticateAsync(request, user);

            if (!response.Success)
            {
                _logger.LogWarning($"Login failed for username {request.Username}: {response.Message}");
                return Unauthorized(response);
            }

            _logger.LogInformation($"Login successful for username: {request.Username}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.GetType().Name} - {ex.Message}");
            return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Register a new user
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation($"Registration attempt for username: {request.Username}, email: {request.Email}");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            _logger.LogWarning($"Invalid model state for registration: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
            return BadRequest(ModelState);
        }

        try
        {
            // Initialize indexes on first use
            await _userRepository.InitializeIndexesAsync();

            // Check if user already exists
            _logger.LogInformation($"Checking if username '{request.Username}' already exists");
            if (await _userRepository.UserExistsAsync(request.Username))
            {
                _logger.LogWarning($"Registration failed: Username '{request.Username}' already exists");
                return BadRequest(new AuthResponse { Success = false, Message = "Username already taken" });
            }

            // Validate registration
            _logger.LogInformation($"Validating registration for username: {request.Username}");
            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
            {
                _logger.LogWarning($"Registration validation failed for username '{request.Username}': {response.Message}");
                return BadRequest(response);
            }

            // Create the user
            _logger.LogInformation($"Creating user account for username: {request.Username}");
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHashService.HashPassword(request.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateUserAsync(user);
            _logger.LogInformation($"User '{request.Username}' registered successfully with ID {createdUser.Id}");

            return Ok(new AuthResponse { Success = true, Message = "Registration successful. Please log in." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error for username '{request.Username}': {ex.GetType().Name} - {ex.Message}");
            if (ex.InnerException != null)
                _logger.LogError($"Inner exception: {ex.InnerException.Message}");

            return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Validate current JWT token
    /// GET /api/auth/validate
    /// </summary>
    [HttpGet("validate")]
    public ActionResult<object> ValidateToken()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { message = "No token provided" });

        // Token validation handled by middleware
        return Ok(new { message = "Token is valid" });
    }
}
