namespace Payments.API.Messaging
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(string queue, T message);

       
    }
}
