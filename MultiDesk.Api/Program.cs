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
    options.UseNpgsql(builder.Configuration.GetConnectionString("MultiDeskDatabase")));

// Repository pattern — scoped matches DbContext lifetime
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();