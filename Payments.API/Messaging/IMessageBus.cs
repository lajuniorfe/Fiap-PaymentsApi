namespace Payments.API.Messaging
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(string queue, T message);

        Task SubscribeAsync<T>(
            string queue,
            Func<T, Task> handler);
    }
}
