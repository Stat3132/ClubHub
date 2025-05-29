public class OrderNotification
{
    public required Guid userID { get; set; }
    public required Guid clubID { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Message { get; set; }
}