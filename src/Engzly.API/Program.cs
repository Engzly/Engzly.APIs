using Engzly.API;
using Engzly.API.Middlewares;
using Engzly.Application;
using Engzly.Infrastructure;
using Engzly.Infrastructure.Persistence.Data;
using Engzly.Infrastructure.Persistence.Seeders;
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

app.Run();

#endregion
