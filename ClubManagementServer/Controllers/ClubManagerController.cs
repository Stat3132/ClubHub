using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Net;
using ClubManagementServer.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ClubManagementServer.Models;

[ApiController]
[Route("api/[controller]")]
public class ClubManagerController : ControllerBase
{
    private readonly ClubHubDBContext _dbContext;

    public ClubManagerController(ClubHubDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    // POST: api/ClubManager/join-request
    [HttpPost("join-request")]
    [AllowAnonymous]
    public IActionResult RequestToJoinClub([FromBody] ClubJoinRequest request)
    {
        request.JoinRequestID = Guid.NewGuid();
        request.RequestDate = DateTime.UtcNow;
        _dbContext.club_join_request.Add(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Join request submitted." });
    }

    // POST: api/ClubManager/create-request
    [HttpPost("create-request")]
    [AllowAnonymous]
    public IActionResult RequestToCreateClub([FromBody] ClubCreateRequest request)
    {
        request.CreateRequestID = Guid.NewGuid();
        request.RequestDate = DateTime.UtcNow;
        _dbContext.club_create_request.Add(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Create club request submitted." });
    }

    // GET: api/ClubManager/join-requests
    [HttpGet("join-requests")]
    [Authorize]
    public IActionResult GetJoinRequests()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (role != "Advisor" && role != "Admin")
        {
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can access all users." });
        }

        var requests = _dbContext.club_join_request.ToList();
        return Ok(requests);
    }

    // DELETE: api/ClubManager/join-request/{id}
    [HttpDelete("join-request/{id}")]
    [Authorize]
    public IActionResult DeleteJoinRequest(Guid id)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (role != "Advisor" && role != "Admin")
        {
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can access all users." });
        }

        var request = _dbContext.club_join_request.Find(id);
        if (request == null)
            return NotFound(new { Success = false, Message = "Join request not found." });

        _dbContext.club_join_request.Remove(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Join request deleted." });
    }

    // GET: api/ClubManager/create-requests
    [HttpGet("create-requests")]
    [Authorize]
    public IActionResult GetCreateRequests()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (role != "Advisor" && role != "Admin")
        {
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can access all users." });
        }

        var requests = _dbContext.club_create_request.ToList();
        return Ok(requests);
    }

    // DELETE: api/ClubManager/create-request/{id}
    [HttpDelete("create-request/{id}")]
    [Authorize]
    public IActionResult DeleteCreateRequest(Guid id)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (role != "Advisor" && role != "Admin")
        {
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can access all users." });
        }

        var request = _dbContext.club_create_request.Find(id);
        if (request == null)
            return NotFound(new { Success = false, Message = "Create request not found." });

        _dbContext.club_create_request.Remove(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Create club request deleted." });
    }

    // Accept Join Request
    [HttpPost("accept-join-request/{id}")]
    [Authorize]
    public IActionResult AcceptJoinRequest(Guid id)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "Advisor" && role != "Admin")
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can accept join requests." });

        var request = _dbContext.club_join_request.Find(id);
        if (request == null)
            return NotFound(new { Success = false, Message = "Join request not found." });

        // Add user to club (userclub table)
        var user = _dbContext.user.FirstOrDefault(u => u.email == request.StudentEmail);
        if (user == null)
            return NotFound(new { Success = false, Message = "User not found." });

        var userClub = new UserClub
        {
            userClubID = Guid.NewGuid(),
            userID = user.userID,
            clubID = request.ClubID
        };
        _dbContext.userclub.Add(userClub);

        // Remove the join request
        _dbContext.club_join_request.Remove(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Join request accepted and user added to club." });
    }

    // Deny Join Request
    [HttpPost("deny-join-request/{id}")]
    [Authorize]
    public IActionResult DenyJoinRequest(Guid id)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "Advisor" && role != "Admin")
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can deny join requests." });

        var request = _dbContext.club_join_request.Find(id);
        if (request == null)
            return NotFound(new { Success = false, Message = "Join request not found." });

        _dbContext.club_join_request.Remove(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Join request denied and deleted." });
    }

    // Accept Create Request
    [HttpPost("accept-create-request/{id}")]
    [Authorize]
    public IActionResult AcceptCreateRequest(Guid id, [FromQuery] string advisorEmail)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "Advisor" && role != "Admin")
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can accept create requests." });

        var request = _dbContext.club_create_request.Find(id);
        if (request == null)
            return NotFound(new { Success = false, Message = "Create request not found." });

        // Look up president by email
        var president = _dbContext.user.FirstOrDefault(u => u.email == request.StudentEmail);
        if (president == null)
        {
            // Optionally, create the user if not found
            president = new User
            {
                userID = Guid.NewGuid(),
                firstName = request.StudentName.Split(' ')[0],
                lastName = request.StudentName.Contains(" ") ? request.StudentName.Substring(request.StudentName.IndexOf(" ") + 1) : "",
                email = request.StudentEmail,
                phoneNumber = "", // Set as needed
                password = new byte[0], // Set as needed
                role = Role.President
            };
            _dbContext.user.Add(president);
            _dbContext.SaveChanges();
        }

        // Example: Assume advisorEmail is passed in the ClubCreateRequest
        Console.WriteLine("Advisor Email: " + advisorEmail);
        var advisor = _dbContext.user.FirstOrDefault(u => u.email == advisorEmail);
        if (advisor == null)
        {
            return BadRequest(new { Success = false, Message = "Advisor not found." });
        }

        // Create the club
        var club = new Club
        {
            clubID = Guid.NewGuid(),
            clubName = request.ClubName,
            clubDeclaration = request.ClubDeclaration,
            presidentName = request.StudentName,
            presidentID = president.userID, // You may want to look up or create the user and use their ID
            advisorID = advisor.userID    // Set appropriately
        };
        _dbContext.club.Add(club);

        // Remove the create request
        _dbContext.club_create_request.Remove(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Create request accepted and club created." });
    }

    // Deny Create Request
    [HttpPost("deny-create-request/{id}")]
    [Authorize]
    public IActionResult DenyCreateRequest(Guid id)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "Advisor" && role != "Admin")
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can deny create requests." });

        var request = _dbContext.club_create_request.Find(id);
        if (request == null)
            return NotFound(new { Success = false, Message = "Create request not found." });

        _dbContext.club_create_request.Remove(request);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "Create request denied and deleted." });
    }

    // DELETE: api/ClubManager/userclub
    [HttpDelete("userclub")]
    [Authorize]
    public IActionResult DeleteUserFromClub([FromQuery] Guid userID, [FromQuery] Guid clubID)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "Advisor" && role != "Admin")
            return StatusCode(403, new { Success = false, Message = "Only advisors and admins can remove users from clubs." });

        var userClub = _dbContext.userclub.FirstOrDefault(uc => uc.userID == userID && uc.clubID == clubID);
        if (userClub == null)
            return NotFound(new { Success = false, Message = "User is not a member of the specified club." });

        _dbContext.userclub.Remove(userClub);
        _dbContext.SaveChanges();
        return Ok(new { Success = true, Message = "User removed from club." });
    }
}
