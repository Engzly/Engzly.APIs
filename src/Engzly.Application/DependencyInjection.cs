using System.Reflection;
using Engzly.Application.Common.Behaviors;
using Engzly.Application.Common.Moderation;
using Engzly.Application.Interfaces.Moderation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Engzly.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MessageModerationBehavior<,>));
        services.AddSingleton<IContentPolicy, RegexContentPolicy>();
        services.AddHttpContextAccessor();
        return services;
    }
}