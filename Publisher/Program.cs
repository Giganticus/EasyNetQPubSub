using EasyNetQ;
using EasyNetQ.DI;

class Program 
{
    static void Main(string[] args) 
    {
        var connectionString = "host=localhost;username=guest;password=guest";
        
        using (var bus = RabbitHutch.CreateBus(
                   connectionString,
                   services => services.Register<IConventions>(
                       c => 
                       new MyConventions(c.Resolve<ITypeNameSerializer>()),
                       Lifetime.Singleton))) 
        {
            var input = String.Empty;
            Console.WriteLine("Enter a message. 'Quit' to quit.");
            while ((input = Console.ReadLine()) != "Quit")
            {
                var exchange = bus.Advanced.ExchangeDeclare("my.existing.exchange", conf =>
                {
                    conf.WithType("direct");
                });
                IMessage message = new Message<string>(input);
                bus.Advanced.PublishAsync(exchange, "#", false, message, CancellationToken.None);
              
                Console.WriteLine("Message published!");
            }
        }
    }
}

public class MyConventions : Conventions
{
    public MyConventions(ITypeNameSerializer typeNameSerializer) : base(typeNameSerializer)
    {
        RpcRequestExchangeNamingConvention = x => "my.existing.exchange";
        RpcResponseExchangeNamingConvention = x => "my.existing.exchange";
        ExchangeNamingConvention = x => "my.existing.exchange";
    }
}