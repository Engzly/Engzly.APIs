using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Notifications;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Interfaces.Services.Engzly.Application.Interfaces;
using Engzly.Domain.Entities.Identity;
using Engzly.Infrastructure.Authentication;
using Engzly.Infrastructure.Authorization.OtpSecurity;
using Engzly.Infrastructure.Authorization.OtpSecurity.Notification;
using Engzly.Infrastructure.Blobs;
using Engzly.Infrastructure.Notifications;
using Engzly.Infrastructure.Persistence.Data;
using Engzly.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Engzly.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDbContext<EngzlyDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<User, IdentityRole>(option =>
        {
            option.Password.RequireDigit = true;
            option.Password.RequireLowercase = true;
            option.Password.RequireNonAlphanumeric = true;
            option.Password.RequireUppercase = true;
            option.Password.RequiredLength = 6;
            option.Password.RequiredUniqueChars = 1;

            // Lockout settings.
            option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            option.Lockout.MaxFailedAccessAttempts = 5;
            option.Lockout.AllowedForNewUsers = true;

            // User settings.
            option.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            //option.User.RequireUniqueEmail = true;
            option.SignIn.RequireConfirmedEmail = true;

        })
            .AddEntityFrameworkStores<EngzlyDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileService, FileService>();

        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IWhatsAppSender, WhatsAppSender>();
        services.AddHttpClient<IWhatsAppSender, WhatsAppSender>();
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((context, options) =>
            {
                options.Host("rabbitmq://localhost");
                options.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

        services.AddLogging(configuration, environment);

        return services;
    }

    private static IServiceCollection AddLogging(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSerilog(options =>
        {
            options.Enrich.FromLogContext()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithMachineName();

            options.WriteTo.Seq(configuration.GetConnectionString("Seq")!)
                    .WriteTo.Console();

            options.MinimumLevel.Is(
                    environment.IsDevelopment()
                        ? LogEventLevel.Debug
                        : LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning);
        });

        return services;
    }
}