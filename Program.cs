﻿using AppMvc.Services;
using AppMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using App.Data;
using Microsoft.Extensions.FileProviders;
using App.Menu;
using Microsoft.AspNetCore.Mvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();           // Đăng ký dịch vụ lưu cache trong bộ nhớ (Session sẽ sử dụng nó)
builder.Services.AddSession(cfg => {                    // Đăng ký dịch vụ Session
    cfg.Cookie.Name = "buivandat";             // Đặt tên Session - tên này sử dụng ở Browser (Cookie)
    cfg.IdleTimeout = new TimeSpan(0,30, 0);    // Thời gian tồn tại của Session
});
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppMvcConnectstring"));
});
builder.Services.AddOptions();
var mailsetting = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddSingleton<IEmailSender, SendMailService>();
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true; // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false; // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;

});

// Cấu hình Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    // options.Cookie.HttpOnly = true;  
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = $"/login/";                                 // Url đến trang đăng nhập
    options.LogoutPath = $"/logout/";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";   // Trang khi User bị cấm truy cập
});
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Trên 5 giây truy cập lại sẽ nạp lại thông tin User (Role)
    // SecurityStamp trong bảng User đổi -> nạp lại thông tinn Security
    options.ValidationInterval = TimeSpan.FromSeconds(5);
});
builder.Services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        var gconfig = builder.Configuration.GetSection("Authentication:Google");
                        options.ClientId = gconfig["ClientId"];
                        options.ClientSecret = gconfig["ClientSecret"];
                        // https ://localhost:7191/signin-google
                        options.CallbackPath = "/dang-nhap-tu-google";
                    })
                    .AddFacebook(options =>
                    {
                        var fconfig = builder.Configuration.GetSection("Authentication:Facebook");
                        options.AppId = fconfig["AppId"];
                        options.AppSecret = fconfig["AppSecret"];
                        options.CallbackPath = "/dang-nhap-tu-facebook";
                    });
                    // .AddTwitter()
                    // .AddMicrosoftAccount()
                    
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", builder =>
    {
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});
builder.Services.AddTransient<CartService>();


builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", builder =>
    {
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});
builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddTransient<AdminSidebarService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
                  Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
              ),
    RequestPath = "/contents"
});
app.UseSession();
app.UseRouting();
app.UseAuthentication(); // xac thuc danh tinh
app.UseAuthorization(); // xac thuc quyen truy cap

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
