using Quasar.Common.Messages;

namespace Quasar.Client.MessageHandlers
{
    public abstract class NotificationMessageProcessor : MessageProcessorBase<string>
    {
        protected NotificationMessageProcessor() : base(true)
        {
        }
    }
}
