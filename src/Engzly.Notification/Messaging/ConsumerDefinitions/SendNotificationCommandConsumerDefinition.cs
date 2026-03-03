using Engzly.Notification.Messaging.Consumers;
using MassTransit;

namespace Engzly.Notification.Messaging.ConsumerDefinitions;

internal sealed class SendNotificationCommandConsumerDefinition : ConsumerDefinition<SendNotificationCommandConsumer>
{
    public SendNotificationCommandConsumerDefinition()
    {
        EndpointName = "send-notification";
    }
    
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SendNotificationCommandConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Immediate(3));
    }
}