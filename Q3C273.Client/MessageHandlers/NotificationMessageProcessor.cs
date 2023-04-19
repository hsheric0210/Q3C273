using Q3C273.Shared.Messages;

namespace Ton618.MessageHandlers
{
    public abstract class NotificationMessageProcessor : MessageProcessorBase<string>
    {
        protected NotificationMessageProcessor() : base(true)
        {
        }
    }
}
