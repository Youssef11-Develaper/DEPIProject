using Maw3dyCare.Models.Maw3dyCareDB;
using Microsoft.AspNetCore.Identity;

namespace Maw3dyCare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddSignalR();

            // تسجيل الـ DbContext
            builder.Services.AddDbContext<Maw3dyCareDB>();

            // Add services to the container
            builder.Services.AddControllersWithViews();

            // تسجيل Identity مع ApplicationAdmin
            builder.Services.AddIdentity<ApplicationAdmin, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<Maw3dyCareDB>()
            .AddDefaultTokenProviders();

            // إعداد الـ Cookie بتاع الأدمن
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.AccessDeniedPath = "/Auth/Login";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });

            // إضافة Cookie الخاص بالمستشفيات
            builder.Services.AddAuthentication()
                .AddCookie("HospitalCookie", options =>
                {
                    options.LoginPath = "/HospitalAuth/Login";
                    options.AccessDeniedPath = "/HospitalAuth/Login";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.Cookie.Name = "HospitalSession";
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapHub<Maw3dyCare.Hubs.ReservationHub>("/reservationHub");
            // ← الصفحة الرئيسية هتبدأ بـ Landing
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Maw3dyCare}/{action=Landing}/{id?}");

            // Route للمستشفى
            app.MapControllerRoute(
                name: "hospital",
                pattern: "hospital/{controller=HospitalAuth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
//test push
