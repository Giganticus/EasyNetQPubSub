using System.Text;
using EasyNetQ;
using EasyNetQ.DI;

class Program 
{
    static void Main(string[] args)
    {
        var connectionString = "host=localhost;username=guest;password=guest";
        
        using (var bus = RabbitHutch.CreateBus(
                   connectionString,
                   services =>
                   {
                       services.Register<IConventions>(c =>
                           new MyConventions(c.Resolve<ITypeNameSerializer>()));

                       services.Register<ITypeNameSerializer>(c => new MyTypeNameSerializer());

                       services.Register<IMessageSerializationStrategy>(
                           c => new MyMessageSerializationStrategy());
                   }))
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

public class MyTypeNameSerializer : ITypeNameSerializer
{
    public string Serialize(Type type)
    {
        return type.ToString();
    }

    public Type DeSerialize(string typeName)
    {
        return typeof(string);
    }
}

public class MyMessageSerializationStrategy : IMessageSerializationStrategy
{
    private readonly ITypeNameSerializer _typeNameSerializer;
    private readonly ISerializer _serializer;

    public MyMessageSerializationStrategy(
        ITypeNameSerializer typeNameSerializer,
        ISerializer serializer)
    {
        _typeNameSerializer = typeNameSerializer;
        _serializer = serializer;
    }
    
    public SerializedMessage SerializeMessage(IMessage message)
    {
        var stringType = typeof(string);
        
        var messageBody = _serializer.MessageToBytes(stringType, message.GetBody());

        var messageProperties = message.Properties;
        messageProperties.Type = _typeNameSerializer.Serialize(stringType);

        return new SerializedMessage(messageProperties, messageBody);
    }

    public IMessage DeserializeMessage(MessageProperties properties, in ReadOnlyMemory<byte> body)
    {
        var messageType = typeof(string);
        var messageBody = Encoding.UTF8.GetString(body.ToArray());
        return MessageFactory.CreateInstance(messageType, messageBody, properties);
    }
}
