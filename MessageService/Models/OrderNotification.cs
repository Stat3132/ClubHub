public class OrderNotification
{
    public Guid UserGuid { get; set; }
    public Guid OrderGuid { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
}