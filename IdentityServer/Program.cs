using Duende.IdentityServer.EntityFramework.Options;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Security.Claims;
using UI;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var identityConnectionString = config.GetConnectionString("DefaultConnection");
var identityServerConnectionString = config.GetConnectionString("IdentityServerConnection");
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

// Add services to the container.
builder.Services.AddRazorPages();
//builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(identityConnectionString));
builder.Services.AddDbContext<IdDataContext>(x => x.UseInMemoryDatabase("Memory"));


builder.Services.AddIdentity<User, Role>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 3;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireLowercase = true;
    //opt.User.RequireUniqueEmail = true;
    //opt.SignIn.RequireConfirmedAccount = true;
    //opt.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    /*opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    opt.Lockout.MaxFailedAccessAttempts = 5;
    opt.Lockout.AllowedForNewUsers = true;

    // User settings.
    opt.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    opt.User.RequireUniqueEmail = false;*/
})
.AddEntityFrameworkStores<IdDataContext>();

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    //options.Authentication.CookieLifetime = TimeSpan.FromMinutes(30);
    //options.Authentication.CookieSlidingExpiration = false;
    options.EmitStaticAudienceClaim = true;
    //options.Authentication.CookieSlidingExpiration = true;
})
    /*.AddConfigurationStore(options => {
        options.ConfigureDbContext = b => b.UseSqlServer(identityServerConnectionString,
            sql => sql.MigrationsAssembly(migrationsAssembly));
        })
    .AddOperationalStore(AddConfigurationOption);*/

    .AddAspNetIdentity<User>()
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddDeveloperSigningCredential();
//.AddTestUsers(TestUsers.Users);
void AddConfigurationOption(OperationalStoreOptions options) {
    options.ConfigureDbContext = b => b.UseSqlServer(identityServerConnectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
                // cleans expired tokens from the database
                //options.EnableTokenCleanup = true;
                //options.TokenCleanupInterval = 3600;
}
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.Name = "IdentityServer.Cookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // used to allow ids to issue new access token without asking users for credentials
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10); 

    options.LoginPath = "/Account/Login/Index";
    options.SlidingExpiration = false;
});

builder.Services.AddAuthentication();

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.MinimumLevel.Debug()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
      .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
      .MinimumLevel.Override("System", LogEventLevel.Warning)
      .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        theme: AnsiConsoleTheme.Code)
      .Enrich.FromLogContext();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseCors(builder => builder
                 .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins("http://localhost:4200"));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var user = new User();
    user.UserName = "Bob";
    user.FirstName = "Bob";
    user.LastName = "Smith";
    var role = new Role { Name = "Admin" };
    var manager = new Role { Name = "Manager" };
    _ = userManager.CreateAsync(user, "bob").GetAwaiter().GetResult();
    _ = roleManager.CreateAsync(role).GetAwaiter().GetResult();
    _ = roleManager.CreateAsync(manager).GetAwaiter().GetResult();
    _ = userManager.AddClaimAsync(user, new Claim("My_Claim", "My_claim_test")).GetAwaiter().GetResult();
    _ = roleManager.AddClaimAsync(role, new Claim("Permission", "projects.view")).GetAwaiter().GetResult();
    _ = roleManager.AddClaimAsync(role, new Claim("Permission", "projects.edit")).GetAwaiter().GetResult();
    _ = roleManager.AddClaimAsync(manager, new Claim("Permission", "projects.view")).GetAwaiter().GetResult();
    _ = roleManager.AddClaimAsync(manager, new Claim("Permission", "projects.create")).GetAwaiter().GetResult();
    _ = userManager.AddToRoleAsync(user, role.Name).GetAwaiter().GetResult();
    _ = userManager.AddToRoleAsync(user, manager.Name).GetAwaiter().GetResult();
}

app.Run();
