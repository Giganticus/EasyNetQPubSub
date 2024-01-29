using System.Text;
using EasyNetQ;

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