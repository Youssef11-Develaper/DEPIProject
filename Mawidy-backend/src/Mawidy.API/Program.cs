using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Mawidy.Application.Interfaces;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Application.Features.Courts.Commands;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBookingCommand).Assembly));

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseInMemoryDatabase("MawidyDb"));

builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed Database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    Mawidy.API.Data.DbInitializer.SeedAsync(context).Wait();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Mawidy API";
        options.Theme = ScalarTheme.Mars;
    });
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

