using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuitionPaymentAPI.Data;
using TuitionPaymentAPI.DTOs;
using TuitionPaymentAPI.Services;

namespace TuitionPaymentAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly TuitionDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, TuitionDbContext context, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token with expiration</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning($"Failed login attempt for username: {request.Username}");
            return Unauthorized(new { error = new { code = "INVALID_CREDENTIALS", message = "Invalid username or password" } });
        }

        var token = _tokenService.GenerateToken(user);
        var jwtSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("JwtSettings");
        var expirationHours = jwtSettings.GetValue<int>("ExpirationHours", 24);

        _logger.LogInformation($"Successful login for username: {user.Username}");

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresIn = expirationHours * 3600, // Convert hours to seconds
            Username = user.Username,
            Role = user.Role
        });
    }
}
