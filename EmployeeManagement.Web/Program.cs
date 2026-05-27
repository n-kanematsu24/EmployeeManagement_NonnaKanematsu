using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Repository;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddLocalization(options => options.ResourcesPath = "");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();

var app = builder.Build();

var supportedCultures = new[] {
    "ja", "en", "zh-TW", "zh-CN", "ko", "es", "fr", "de", "pt", "it", "vi", "th"
};
var locOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("ja")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
locOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

app.UseRequestLocalization(locOptions);
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
