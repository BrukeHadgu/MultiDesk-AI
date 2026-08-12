using Microsoft.EntityFrameworkCore;
using MultiDesk.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<MultiDeskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MultiDeskDatabase")));

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();