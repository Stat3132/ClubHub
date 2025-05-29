using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Models;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UsersController> _logger;
    private readonly ClubHubDBContext _db;
    private readonly IMapper _mapper;

    public UsersController(ILogger<UsersController> logger, ClubHubDBContext db, IMapper mapper, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _db = db;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
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

            var client = _httpClientFactory.CreateClient();
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

    [HttpPut("{userID:guid}")]
    public async Task<IActionResult> Update(Guid userID, [FromBody] UserDTO userDTO)
    {
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

    [HttpDelete("{userID:guid}")]
    public async Task<IActionResult> Delete(Guid userID)
    {
        try
        {
            var user = await _db.user.FirstOrDefaultAsync(u => u.userID == userID);
            if (user is null)
                return NotFound(new { Success = false, Message = "User not found." });

            _db.user.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "User deleted.", userID = user.userID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user.");
            return StatusCode(500, "Internal server error.");
        }
    }
}
