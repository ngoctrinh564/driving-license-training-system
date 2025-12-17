using dacn_gplx.Models;
using dacn_gplx.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
// ==================== Add Services ====================

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<QuanLyGplxContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// EmailService (Dependency Injection)
builder.Services.AddScoped<EmailService>();

//Face
builder.Services.AddScoped<FaceValidator>();

//ReportService
builder.Services.AddScoped<ReportService>();


var app = builder.Build();

// ==================== Middleware ====================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session phải trước Authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
// ==================== CHẶN TRUY CẬP ADMIN ====================
app.Use(async (context, next) =>
{
    var roleId = context.User.FindFirst("RoleId")?.Value;

    // Nếu vào /Admin nhưng role != 1
    if (context.Request.Path.StartsWithSegments("/Admin") && roleId != "1")
    {
        context.Response.Redirect("/User/Index");
        return;
    }

    await next();
});

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
