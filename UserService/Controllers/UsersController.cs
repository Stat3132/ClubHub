using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace UserService.Controllers;



[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UsersController> _logger;
    private readonly ClubHubDBContext _db;
    private readonly IMapper _mapper;

    private readonly IConfiguration _configuration;

    public UsersController(
        ILogger<UsersController> logger,
        ClubHubDBContext db,
        IMapper mapper,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _db = db;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        // Only allow advisors and admins
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (role != "Advisor" && role != "Admin")
        {
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can access all users." });
        }

        try
        {
            var users = await _db.user.ToListAsync();
            return Ok(new { Success = true, Message = "All users returned.", Users = users });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{userID:guid}")]
    public async Task<IActionResult> GetById(Guid userID)
    {
        try
        {
            var user = await _db.user.FirstOrDefaultAsync(u => u.userID == userID);
            if (user == null)
                return NotFound(new { Success = false, Message = "User not found." });

            return Ok(new { Success = true, Message = "User fetched.", User = user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by ID.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserDTO userDTO)
    {
        try
        {
            var existingUser = await _db.user.FirstOrDefaultAsync(u => u.email == userDTO.email);
            if (existingUser is not null)
                return Ok(new
                {
                    Success = true,
                    Message = "User already exists.",
                    userID = existingUser.userID,
                    email = existingUser.email
                });

            var user = _mapper.Map<User>(userDTO);

            // Hash the password before saving
            using (var sha256 = SHA256.Create())
            {
                user.password = sha256.ComputeHash(Encoding.UTF8.GetBytes(userDTO.password));
            }

            await _db.user.AddAsync(user);
            await _db.SaveChangesAsync();

            // Send notification to MessageService
            var notification = new
            {
                userID = user.userID,
                clubID = Guid.Empty, // or assign a club if available
                Name = $"{user.firstName} {user.lastName}",
                Email = user.email,
                Message = "Welcome to ClubHub! Your account has been created."
            };

            // Get the admin/service JWT token from config
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "0708D4E0-DBF1-405F-18A7-08DD9F49D6F8"),
                new Claim(JwtRegisteredClaimNames.Email, "testtest@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

            var response = await client.PostAsJsonAsync("http://PRO290OcelotAPIGateway:8080/messageserviceapi/api/messages", notification);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to notify MessageService for user {UserID}", user.userID);
            }

            return Ok(new { Success = true, Message = "User created.", userID = user.userID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        Console.WriteLine($"Login attempt: Email={loginDto.Email}, Password={loginDto.Password}");
        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            return BadRequest(new { Success = false, Message = "Email and password are required." });


        var user = await _db.user.FirstOrDefaultAsync(u => u.email == loginDto.Email);
        Console.WriteLine(user.email);
        Console.WriteLine(user.role);

        if (user == null)
            return Unauthorized(new { Success = false, Message = "Invalid credentials." });

        // Hash the provided password and compare
        using (var sha256 = SHA256.Create())
        {
            var hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            Console.WriteLine($"Password hashed: {BitConverter.ToString(hashed).Replace("-", "")}");
            if (!hashed.SequenceEqual(user.password))
                return Unauthorized(new { Success = false, Message = "Invalid credentials." });
        }

        // Generate JWT
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.userID.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.email),
            new Claim("name", $"{user.firstName} {user.lastName}"),
            new Claim(ClaimTypes.Role, user.role.ToString())
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { Success = true, Token = tokenString });
    }


    [HttpPut("{userID:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid userID, [FromBody] UserDTO userDTO)
    {
        // Only allow advisors and admins
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (role != "Advisor" && role != "Admin")
        {
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can access all users." });
        }

        try
        {
            var user = await _db.user.FirstOrDefaultAsync(u => u.userID == userID);
            if (user is null)
                return NotFound(new { Success = false, Message = "User not found." });

            _mapper.Map(userDTO, user);

            // Hash the password before saving
            using (var sha256 = SHA256.Create())
            {
                user.password = sha256.ComputeHash(Encoding.UTF8.GetBytes(userDTO.password));
            }

            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "User updated.", userID = user.userID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
    _logger.LogInformation($"🔴 Attempt to delete user with ID: {id}");

    var user = await _db.user.FindAsync(id); // or _db.Users if your DbSet is named that
    if (user == null)
    {
        _logger.LogWarning($"⚠️ User not found: {id}");
        return NotFound(new { Success = false, Message = "User not found." });
    }

    _db.user.Remove(user);
    await _db.SaveChangesAsync();

    _logger.LogInformation($"✅ User removed: {id}");
    return Ok(new { Success = true, Message = "User deleted." });
}



}
