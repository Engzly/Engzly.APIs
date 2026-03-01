using Engzly.API;
using Engzly.API.Middlewares;
using Engzly.Application;
using Engzly.Infrastructure;
using Engzly.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add Services
// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
#endregion

#region Apply Migrations
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<EngzlyDbContext>();
var migrations = context.Database.GetPendingMigrations();
if (migrations.Any())
    await context.Database.MigrateAsync();
#endregion


#region Configure Middlewares
if (true || app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
