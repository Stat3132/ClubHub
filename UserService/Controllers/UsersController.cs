using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly OrderServiceDBContext _db;
    private readonly IMapper _mapper;

    public UsersController(ILogger<UsersController> logger, OrderServiceDBContext db, IMapper mapper)
    {
        _logger = logger;
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _db.Users.ToListAsync();
            return Ok(new { Success = true, Message = "All users returned.", Users = users });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("withorders")]
    public async Task<IActionResult> GetWithOrders()
    {
        try
        {
            var users = await _db.Users.Include(u => u.Orders).ToListAsync();
            return Ok(new { Success = true, Message = "Users with orders returned.", Users = users });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with orders.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{userGuid:guid}")]
    public async Task<IActionResult> GetById(Guid userGuid)
    {
        try
        {
            var user = await _db.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.UserGuid == userGuid);
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
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);
            if (existingUser is not null)
                return Ok(new
                {
                    Success = true,
                    Message = "User already exists.",
                    UserGuid = existingUser.UserGuid,
                    Email = existingUser.Email
                });

            var user = _mapper.Map<User>(userDTO);
            user.CreatedDate = DateTime.UtcNow;

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "User created.", UserGuid = user.UserGuid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPut("{userGuid:guid}")]
    public async Task<IActionResult> Update(Guid userGuid, [FromBody] UserDTO userDTO)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
                return NotFound(new { Success = false, Message = "User not found." });

            // Map incoming DTO properties onto the existing user entity
            _mapper.Map(userDTO, user);
            // Optional: if you prefer manually updating specific properties, you could do:
            // user.Username = userDTO.Username;
            // user.Email = userDTO.Email;
            // user.Password = userDTO.Password;

            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "User updated.", UserGuid = user.UserGuid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpDelete("{userGuid:guid}")]
    public async Task<IActionResult> Delete(Guid userGuid)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
                return NotFound(new { Success = false, Message = "User not found." });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "User deleted.", UserGuid = user.UserGuid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user.");
            return StatusCode(500, "Internal server error.");
        }
    }
}
