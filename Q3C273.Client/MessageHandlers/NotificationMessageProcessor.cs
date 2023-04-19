using Q3C273.Shared.Messages;

namespace Everything.MessageHandlers
{
    public abstract class NotificationMessageProcessor : MessageProcessorBase<string>
    {
        protected NotificationMessageProcessor() : base(true)
        {
        }
    }
}
