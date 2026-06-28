using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TelecomBranches.Application.Interfaces;
using TelecomBranches.Application.Services;
using TelecomBranches.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Resolve Mawidy-frontend path ─────────────────────────────────────────────
var frontendPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "Mawidy-frontend"));

// ── Add Mawidy-frontend as a file provider so Razor can discover views ───────
builder.Environment.ContentRootFileProvider = new CompositeFileProvider(
    builder.Environment.ContentRootFileProvider,
    new PhysicalFileProvider(frontendPath)
);

// ── Set web root to Mawidy-frontend/wwwroot for static files ─────────────────
builder.Environment.WebRootPath = Path.Combine(frontendPath, "wwwroot");

// ── Razor Views: configure view location formats ─────────────────────────────
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");

        options.AreaViewLocationFormats.Clear();
        options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("TelecomBranches.Infrastructure")
      )
);

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IBranchService,      BranchService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAdminService,       AdminService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// الـ default الآن هو HomeController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
