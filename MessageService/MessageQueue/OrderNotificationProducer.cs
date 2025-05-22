using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

public class OrderNotificationProducer : IOrderNotificationProducer
{
    private readonly IConfiguration _iconfig;

    public OrderNotificationProducer(IConfiguration iconfig)
    {
        _iconfig = iconfig;
    }

    public void SendMessage<T>(T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = _iconfig["RabbitMQ:host"],
            Port = int.Parse(_iconfig["RabbitMQ:port"]),
        };

        var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //channel.QueueDeclare("notifyQueue", exclusive: false);
        channel.QueueDeclare(queue: "notifyQueue", durable: true, exclusive: false, arguments: null, autoDelete: false);

        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(exchange: "", routingKey: "notifyQueue", body: body);
    }
}