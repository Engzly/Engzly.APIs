using Engzly.API;
using Engzly.API.Hubs;
using Engzly.API.Middlewares;
using Engzly.Application;
using Engzly.Domain.Entities.Identity;
using Engzly.Infrastructure;
using Engzly.Infrastructure.Persistence.Data;
using Engzly.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region Add Services

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

#endregion

#region Apply Migrations

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<EngzlyDbContext>();
var migrations = context.Database.GetPendingMigrations();
if (migrations.Any())
    await context.Database.MigrateAsync();

await CategorySeeder.SeedAsync(context);

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
await RoleSeeder.SeedAsync(roleManager, userManager, builder.Configuration);

#endregion



#region Configure Middlewares

if (true || app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

#endregion


#region Map Controllers

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Engzly.API",
    timestamp = DateTime.UtcNow
}))
.AllowAnonymous();

app.Run();

#endregion
