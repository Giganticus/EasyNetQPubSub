using EasyNetQ;
using EasyNetQ.DI;

namespace Subscriber;

internal static class Program 
{
    static void Main()
    {
        const string connectionString = "host=localhost;username=guest;password=guest";

        using var bus = RabbitHutch.CreateBus(
            connectionString,
            services =>
            {
                services.Register<IConventions>(c =>
                    new MyConventions(c.Resolve<ITypeNameSerializer>()));

                services.Register<ITypeNameSerializer>(c => new MyTypeNameSerializer());

                services.Register<IMessageSerializationStrategy>(
                    c => new MyMessageSerializationStrategy(
                        c.Resolve<ITypeNameSerializer>(),
                        c.Resolve<ISerializer>()));
            });
        
        bus.PubSub.Subscribe<string>("test", HandleTextMessage, config =>
        {
            config.WithExchangeType("direct");
        }, CancellationToken.None);
         
        Console.WriteLine("Listening for messages. Hit <return> to quit.");
        Console.ReadLine();
    }

    static void HandleTextMessage(string textMessage) 
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Got message: {0}", textMessage);
        Console.ResetColor();
    }
}