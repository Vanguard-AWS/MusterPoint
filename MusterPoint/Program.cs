using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MusterPoint.Client.Pages;
using MusterPoint.Client.Services;
using MusterPoint.Components;
using MusterPoint.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add CORS
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowLocalhost", policy =>
    {
        policy
         .WithOrigins("https://localhost:7219", "http://localhost:5000", "http://localhost:3000")
         .AllowAnyMethod()
       .AllowAnyHeader()
       .AllowCredentials();
    });
});

// Add Controllers
builder.Services.AddControllers();

// Add logging
builder.Services.AddLogging();

// Add MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddScoped<MusterPoint.Services.ListStorageService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<MigrationService>();

// Register client-side services for server-side rendering
builder.Services.AddHttpClient<MusterPoint.Client.Services.AuthService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<MusterPoint.Client.Services.ListStorageService>();
builder.Services.AddScoped<DatasheetService>();

// Add custom services
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<PasswordHashService>();
builder.Services.AddScoped<JwtTokenService>();

// Add Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (!string.IsNullOrEmpty(secretKey))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
         ValidateIssuerSigningKey = true,
   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
    ValidIssuer = jwtSettings["Issuer"],
             ValidateAudience = true,
             ValidAudience = jwtSettings["Audience"],
         ValidateLifetime = false,  // Allow tokens without expiration
           ClockSkew = TimeSpan.FromSeconds(30)
            };
          
            // Log authentication failures
            options.Events = new JwtBearerEvents
      {
    OnAuthenticationFailed = context =>
                {
   var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
     logger.LogError($"Authentication failed: {context.Exception?.Message}");
               return Task.CompletedTask;
                },
 OnTokenValidated = context =>
      {
          var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
         logger.LogInformation("Token validated successfully");
        return Task.CompletedTask;
  },
     OnChallenge = context =>
     {
  var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
         logger.LogError($"Auth challenge: {context.ErrorDescription}");
        return Task.CompletedTask;
         }
    };
        });

    builder.Services.AddAuthorization();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowLocalhost");

// Run database migrations in background
_ = Task.Run(async () =>
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
      var migrationService = scope.ServiceProvider.GetRequiredService<MigrationService>();
    await migrationService.MigrateCountToModelCountAsync();
 }
    }
    catch (Exception ex)
    {
     var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError($"Migration failed: {ex.Message}");
    }
});

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
.AddAdditionalAssemblies(typeof(MusterPoint.Client._Imports).Assembly);

// Map auth controller
app.MapControllers();

app.Run();
