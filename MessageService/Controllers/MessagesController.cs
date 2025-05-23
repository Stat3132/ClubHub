using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Net;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly IMapper _mapper;
    private readonly OrderServiceDBContext _db;
    //private readonly IConfiguration _config;
    private readonly IOrderNotificationProducer _msgQueue;

    public MessagesController(ILogger<MessagesController> logger, OrderServiceDBContext db, IMapper mapper, IOrderNotificationProducer msgQueue)
    {
        _logger = logger;
        _db = db;
        _mapper = mapper;
        _msgQueue = msgQueue;

        var utcNow = DateTime.UtcNow;
        var localNow = DateTime.Now;
        string hostName = Dns.GetHostName();
        string ip = Dns.GetHostEntry(hostName).AddressList.FirstOrDefault()?.ToString() ?? "Unavailable";

        _logger.LogInformation($"[Messages Controller Initialized] Local: {localNow}, UTC: {utcNow}, Host: {hostName}, IP: {ip}");
    }

    [HttpGet("test1")]
    public IActionResult Test1() => Ok("Hello from MessageController");
    

    [HttpGet("test-msg-queue")]
    public IActionResult TestMessageQueue()
    {
        var notification = new OrderNotification
        {
            UserGuid = Guid.NewGuid(),
            OrderGuid = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            Message = "This is a test message from /test-msg-queue."
        };
        _msgQueue.SendMessage(notification);
        _logger.LogInformation($"Sent Message: {notification.UserGuid}, {notification.OrderGuid}");

        return Ok("Test message sent to queue.");
    }

    [HttpPost]
    public IActionResult SendNotification([FromBody] OrderNotification notification)
    {
        if (notification == null)
        {
            return BadRequest("Notification cannot be null.");
        }

        Console.WriteLine($"Received notification for user {notification.Name} ({notification.Email}) about Order {notification.OrderGuid}");
        _msgQueue.SendMessage(notification);
        _logger.LogInformation($"Sent Message to RabbitMsgQueue: {notification.UserGuid}, {notification.OrderGuid}");

        return Ok(new { Status = "Notification received" });
    }

}
