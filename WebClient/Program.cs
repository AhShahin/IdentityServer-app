using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection("IdentityServerSettings"));
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddAuthentication(
  options =>
  {
      //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      //options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
      options.DefaultScheme = "cookie";
      options.DefaultChallengeScheme = "oidc";
  }).AddCookie("cookie", options =>
  {
      options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
      options.Cookie.MaxAge = options.ExpireTimeSpan; // optional
      options.SlidingExpiration = false;
  })
  .AddOpenIdConnect("oidc", options =>
  {
      options.Authority = builder.Configuration["InteractiveServiceSettings:AuthorityUrl"];
      options.ClientId = builder.Configuration["InteractiveServiceSettings:ClientId"];
      options.ClientSecret = builder.Configuration["InteractiveServiceSettings:ClientSecret"];
      options.Scope.Add(builder.Configuration["InteractiveServiceSettings:Scopes:0"]);
      options.ResponseType = "code";
      options.UsePkce = true;
      options.ResponseMode = "query";
      options.SaveTokens = true;
      // two trips to load claims instead of including them in the Id_token
      options.GetClaimsFromUserInfoEndpoint= true;
      // Configure cookie claim mapping
      //options.Scope.Clear();
      //options.Scope.Add("openid"); // requesting scope
      options.Scope.Add("OurClaim");
      options.Scope.Add("roles");
      options.Scope.Add("OnlineStore.update");

      // map User.Claims.My_Claim to User.Claims.Calim_test
      options.ClaimActions.MapUniqueJsonKey("Calim_test", "My_Claim");
      options.ClaimActions.MapUniqueJsonKey("role", "role");
      options.ClaimActions.DeleteClaim("amr");

  });

  builder.Services.AddAuthorization();

  // allow to use http client factory inside Razor pages
  builder.Services.AddHttpClient();

var app = builder.Build();














// Configure the HTTP request pipeline.


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseBrowserLink();
}
else
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
