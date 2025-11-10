using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrlShortener.MVC.Data;
using UrlShortener.MVC.Data.Entities.Identities;
using UrlShortener.Services.Otp;
using UrlShortener.Services.Mail.Mailjet;
using FluentValidation.AspNetCore;
using UrlShortener.MVC.Validation;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddMailjet(builder.Configuration);

builder.Services.AddOtpService(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UrlVMValidator>();

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UrlShortenerConnection")));

builder.Services.AddDbContext<UrlShortenerIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UrlShortenerIdentityConnection")));


builder.Services.AddIdentity<UrlShortenerUser, UrlShortenerRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // Password settings 
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    // Lockout settings 
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.AllowedForNewUsers = true;
    // User settings 
    options.User.RequireUniqueEmail = true;
    // Sign-in settings 
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
}).AddEntityFrameworkStores<UrlShortenerIdentityDbContext>()
    .AddDefaultTokenProviders();

// Google login
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        googleOptions.SaveTokens = true;
    });

// Microsoft login
builder.Services.AddAuthentication()
    .AddMicrosoftAccount(msOpts =>
    {
        msOpts.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        msOpts.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
        msOpts.SaveTokens = true;
    });

// Facebook login
builder.Services.AddAuthentication()
    .AddFacebook(fb =>
    {
        fb.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
        fb.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
        fb.SaveTokens = true;

        // L?y thêm email (m?c ??nh Facebook không luôn tr?)
        fb.Scope.Add("email");
        fb.Fields.Add("email");
        fb.Fields.Add("name");
        fb.Fields.Add("picture");

        // Map avatar vào claim tu? bi?n (n?u mu?n l?u ?nh)
        fb.ClaimActions.MapCustomJson("urn:facebook:picture", user =>
        {
            try
            {
                return user.GetProperty("picture").GetProperty("data").GetProperty("url").GetString();
            }
            catch { return null; }
        });
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    //options.LoginPath = "/Identity/Account/Login";
    options.LoginPath = "/Authentication/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

});

//builder.Services.ConfigureApplicationCookie(o =>
//{
//    o.LoginPath = "/Identity/Account/Login";
//    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
//});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Urls}/{action=Create}");

app.MapRazorPages();

app.Run();
