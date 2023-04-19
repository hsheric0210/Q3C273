using Q3C273.Shared.Messages;

namespace Q3C273.Shared.Networking
{
    public interface ISender
    {
        void Send<T>(T message) where T : IMessage;
        void Disconnect();
    }
}
