using Mawidy.Infrastructure.Persistence;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Infrastructure.Persistence.Repositories;
using Mawidy.Application.Interfaces;
using Mawidy.Application.Services;
using Microsoft.Extensions.FileProviders;
using Mawidy.Infrastructure.Services;
using Mawidy.Application.Banks.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")));



builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Mawidy.Application.Features.Courts.Queries.GetCourtsQuery).Assembly));

builder.Services.AddHostedService<ReminderService>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
})
.AddCookie("MvcCookies", options =>
{
    options.LoginPath = "/Banks/Home/Login";
    options.AccessDeniedPath = "/Banks/Home/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Cookie.Name = ".MvcAuth";
})
.AddCookie("HospitalCookies", options =>
{
    options.LoginPath = "/Hospitals/HospitalAuth/Login";
    options.Cookie.Name = ".HospitalAuth";
    options.AccessDeniedPath = "/Hospitals/HospitalAuth/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://127.0.0.1:5500", "http://localhost:5500",  // main frontend dev server
                "http://localhost:5154",                             // main backend (self)
                "http://localhost:5281",                             // Banks
                "http://localhost:5216"                              // Healthcare
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Services
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IQRService, QRService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<IPeakTimeService, PeakTimeService>();
builder.Services.AddScoped<IAppointmentAvailabilityService, AppointmentAvailabilityService>();
builder.Services.AddHostedService<AppointmentCompletionService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();

// Banks & Hospitals Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LocalizationService>();

// SignalR
builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mawidy API",
        Version = "v1",
        Description = "API للسجل المدني الرقمي - رواد مصر الرقمية"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "ادخل: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed Roles and Admin
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(context);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { Roles.Admin, Roles.BranchAdmin, Roles.Citizen };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@civil.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            FirstName = "المدير",
            LastName = "العام",
            NationalId = "00000000000000",
            UserName = adminEmail,
            Email = adminEmail,
            PhoneNumber = "01000000000",
            EmailConfirmed = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        await userManager.CreateAsync(adminUser, "Test@1234");
        await userManager.AddToRoleAsync(adminUser, Roles.Admin);    var testEmail = "test@mawidy.com";
    var testUser = await userManager.FindByEmailAsync(testEmail);
    if (testUser == null)
    {
        testUser = new ApplicationUser
        {
            FirstName = "Test",
            LastName = "User",
            NationalId = "11111111111111",
            UserName = testEmail,
            Email = testEmail,
            PhoneNumber = "01111111111",
            EmailConfirmed = true,
            DateOfBirth = new DateTime(1995, 1, 1)
        };
        await userManager.CreateAsync(testUser, "Test@1234");
        await userManager.AddToRoleAsync(testUser, Roles.Citizen);
    }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files from Mawidy-frontend first to ensure frontend changes are reflected instantly
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"f:\DEPIProject\Mawidy-frontend"),
    RequestPath = ""
});

// Backend wwwroot → serves CSS/JS for Razor views (~/css/site.css, ~/js/branches.js …)
app.UseStaticFiles();

// Frontend folder → serves index.html and other HTML pages
var fileServerOptions = new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(@"f:\DEPIProject\Mawidy-frontend"),
    RequestPath = "",
    EnableDefaultFiles = true
};
fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseFileServer(fileServerOptions);

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Backward-compatible redirects for old Maw3dyCare URLs (before Areas consolidation)
app.MapGet("/Maw3dyCare/{action}/{id?}", (string action, string? id) =>
    Results.Redirect(id != null ? $"/Hospitals/Maw3dyCare/{action}/{id}" : $"/Hospitals/Maw3dyCare/{action}"));
app.MapGet("/HospitalDashboard/{action}/{id?}", (string action, string? id) =>
    Results.Redirect(id != null ? $"/Hospitals/HospitalDashboard/{action}/{id}" : $"/Hospitals/HospitalDashboard/{action}"));
app.MapGet("/HospitalAuth/{action}/{id?}", (string action, string? id) =>
    Results.Redirect(id != null ? $"/Hospitals/HospitalAuth/{action}/{id}" : $"/Hospitals/HospitalAuth/{action}"));
app.MapGet("/HospitalDashboard", () => Results.Redirect("/Hospitals/HospitalDashboard/Index"));
app.MapGet("/Maw3dyCare", () => Results.Redirect("/Hospitals/Maw3dyCare/Landing"));
app.MapGet("/Maw3dyCare/Hospitals", () => Results.Redirect("/Hospitals/Maw3dyCare/Hospitals"));

// Areas (Banks & Hospitals MVC) - BEFORE default
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// REST API controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<Mawidy.API.Hubs.Banks.BookingHub>("/hubs/booking");
app.MapHub<Mawidy.API.Hubs.Hospitals.ReservationHub>("/hubs/reservation");

app.Run();








