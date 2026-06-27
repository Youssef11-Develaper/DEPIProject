using Microsoft.EntityFrameworkCore;
using Mawidy.Application.Interfaces;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.Features.Courts.Commands;
using Mawidy.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly));

// Register DbContext (InMemory for demo/local dev)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("MawidyDb"));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    await DbInitializer.SeedAsync(context);
}

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Courts}/{action=Index}/{id?}"); // Default route mapping to Courts/Index

app.Run();
