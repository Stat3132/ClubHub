
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;


public class OrderNotificationConsumer : BackgroundService
{
    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private IModel _channel;
    private readonly IConfiguration _iconfig;

    public OrderNotificationConsumer(IConfiguration iconfig)
    {
        _iconfig = iconfig;

        _factory = new ConnectionFactory()
        {
            //similar to saying:  _iconfig.GetConnectionString("DefaultConnection");
            HostName = _iconfig["RabbitMQ:host"],
            Port = int.Parse(_iconfig["RabbitMQ:port"]),
            //UserName = "guest",
            //Password = "password",
            VirtualHost = "/",
        };

        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "notifyQueue", durable: true, exclusive: false, arguments: null, autoDelete: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);

        // these register all the event handlers
        consumer.Shutdown += OnConsumerShutdown;
        consumer.Registered += OnConsumerRegistered;
        consumer.Unregistered += OnConsumerUnregistered;
        consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

        // this is the only event handler we care about - when this consumer.Received is activated by finding a new message in the notifyQueue
        consumer.Received += (model, ea) =>
        {
            WriteTextLog(ea);
            //SendEmail(ea);
        };

        _channel.BasicConsume(queue: "notifyQueue", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private void WriteTextLog(BasicDeliverEventArgs ea)
    {
        // sample logging output of message read from rabbitmq
        Console.WriteLine("OrderNotificationConsumer has received a new message\r\n");
        var body = ea.Body;
        var message = Encoding.UTF8.GetString(body.ToArray());
        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

        Console.WriteLine("Here is the message: \r\n" + message);
        Console.WriteLine();
        OrderNotification orderNotification = JsonConvert.DeserializeObject<OrderNotification>(message);

        Console.WriteLine("Email: " + orderNotification.Email);
        Console.WriteLine();
        Console.WriteLine("Messsage: " + orderNotification.Message);
        Console.WriteLine();

        // TODO: Update code below to use custom body/subject
        string subject = "Welcome to the club!";
        string bodytext = $"User #: {orderNotification.userID}\nClub #: {orderNotification.clubID}\n\nDear {orderNotification.Name},\n\nThank you for Joining the club!\n\n{orderNotification.Message}\n\nSincerely,\n\n THE CLUBHUB";

        Console.WriteLine("Subject: " + subject);
        Console.WriteLine("Body: " + bodytext);

        SendEmail(orderNotification.Email, subject, bodytext);
    }

    private void SendEmail(string to, string subject, string body)
    {
        //Using simple & free Ethereal
        var client = new SmtpClient("smtp.ethereal.email", 587)
        {
            Credentials = new NetworkCredential("alexzander.swift@ethereal.email", "QgjC6qEBKwvMyvN4Qx"),
            EnableSsl = true
        };
        var mail = new MailMessage("from@test.com", to, subject, body);
        client.Send(mail);
    }

    private void OnConsumerConsumerCancelled(object? sender, ConsumerEventArgs e) { }
    private void OnConsumerUnregistered(object? sender, ConsumerEventArgs e) { }
    private void OnConsumerRegistered(object? sender, ConsumerEventArgs e) { }
    private void OnConsumerShutdown(object? sender, ShutdownEventArgs e) { }
    private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e) { }
}