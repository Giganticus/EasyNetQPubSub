using EasyNetQ;
using EasyNetQ.DI;

class Program 
{
    static void Main(string[] args)
    {
        var connectionString = "host=localhost;username=guest;password=guest";
        
        using (var bus = RabbitHutch.CreateBus(
                   connectionString,
                   services => services.Register<IConventions>(c => 
                       new MyConventions(c.Resolve<ITypeNameSerializer>()))))
        {
            bus.PubSub.Subscribe<string>("test", HandleTextMessage, config =>
            {
                config.WithExchangeType("direct");
            }, CancellationToken.None);
         
            Console.WriteLine("Listening for messages. Hit <return> to quit.");
            Console.ReadLine();
        }
    }

    static void HandleTextMessage(string textMessage) 
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Got message: {0}", textMessage);
        Console.ResetColor();
    }
}

public class MyConventions : Conventions
{
    public MyConventions(ITypeNameSerializer typeNameSerializer) : base(typeNameSerializer)
    {
        ExchangeNamingConvention = x => "my.existing.exchange";
    }
}