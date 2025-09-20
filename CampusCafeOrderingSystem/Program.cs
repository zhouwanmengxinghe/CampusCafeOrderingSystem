using CampusCafeOrderingSystem.Data;
using CampusCafeOrderingSystem.Middleware;
using CampusCafeOrderingSystem.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies; // added
using Microsoft.AspNetCore.Http; // added
#nullable enable
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ModelValidationFilter>();
    options.Filters.Add<ApiResponseFilter>();
    options.Filters.Add<RateLimitFilter>();
    options.Filters.Add<InputSanitizationFilter>();
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Campus Cafe Ordering System API",
        Version = "v1",
        Description = "API for Campus Cafe Ordering System"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// Register custom services
builder.Services.AddScoped<CampusCafeOrderingSystem.Services.IMenuService, CampusCafeOrderingSystem.Services.MenuService>();
builder.Services.AddScoped<CampusCafeOrderingSystem.Services.IOrderService, CampusCafeOrderingSystem.Services.OrderService>();
builder.Services.AddScoped<CampusCafeOrderingSystem.Services.ICreditService, CampusCafeOrderingSystem.Services.CreditService>();

// Register HttpClient for OrderService
builder.Services.AddHttpClient<CampusCafeOrderingSystem.Services.OrderService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5117");
});

// Add SignalR
builder.Services.AddSignalR();

// Add caching services
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;

    // Return 401/403 for API and AJAX requests instead of HTML redirect
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api") ||
                string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
                (ctx.Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase)))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api") ||
                string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
                (ctx.Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase)))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Campus Cafe API V1");
        c.RoutePrefix = "api/docs";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the application to run on port 5117
app.Urls.Add("http://localhost:5117");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Add this line - it was missing!
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Map SignalR Hub
app.MapHub<CampusCafeOrderingSystem.Hubs.OrderHub>("/orderHub");

await SeedRolesAndAdminAsync(app);

app.Run();

async Task SeedRolesAndAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roleNames = { "Admin", "Customer", "Vendor" };
    foreach (var role in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "1234@qq.com";
    var adminPassword = "Admin123!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(newAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }

    // Create default vendor account
    var vendorEmail = "vendor@campuscafe.com";
    var vendorPassword = "Vendor123!";
    var vendorUser = await userManager.FindByEmailAsync(vendorEmail);
    if (vendorUser == null)
    {
        var newVendor = new IdentityUser { UserName = vendorEmail, Email = vendorEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(newVendor, vendorPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newVendor, "Vendor");
        }
    }
    
    // Create vendor@qq.com account for testing
    var testVendorEmail = "vendor@qq.com";
    var testVendorPassword = "Vendor123!";
    var testVendorUser = await userManager.FindByEmailAsync(testVendorEmail);
    if (testVendorUser == null)
    {
        var newTestVendor = new IdentityUser { UserName = testVendorEmail, Email = testVendorEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(newTestVendor, testVendorPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newTestVendor, "Vendor");
        }
    }
    
    // Create default customer account for testing
    var customerEmail = "customer@test.com";
    var customerPassword = "Customer123!";
    var customerUser = await userManager.FindByEmailAsync(customerEmail);
    if (customerUser == null)
    {
        var newCustomer = new IdentityUser { UserName = customerEmail, Email = customerEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(newCustomer, customerPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newCustomer, "Customer");
        }
    }
    
    // Seed menu items and orders data
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedData.SeedAsync(context);
}
