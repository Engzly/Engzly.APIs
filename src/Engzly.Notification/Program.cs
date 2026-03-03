using Engzly.Notification.Messaging.ConsumerDefinitions;
using Engzly.Notification.Messaging.Consumers;
using Engzly.Notification.Services;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<INotificationService, FirebaseNotificationService>();
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<SendNotificationCommandConsumer, SendNotificationCommandConsumerDefinition>();
    config.UsingRabbitMq((context, options) =>
    {
        options.Host("rabbitmq://localhost");
        options.ConfigureEndpoints(context);
    });
});


var host = builder.Build();
host.Run();