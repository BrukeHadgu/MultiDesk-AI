using Microsoft.EntityFrameworkCore;
using MultiDesk.Application.Common;
using MultiDesk.Application.Interfaces;
using MultiDesk.Infrastructure.Persistence;
using MultiDesk.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Database
builder.Services.AddDbContext<MultiDeskDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("MultiDeskDatabase")));

// Repository pattern
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Seeder
builder.Services.AddScoped<MultiDeskSeeder>();

var app = builder.Build();

// Run seeder on startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MultiDeskSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();