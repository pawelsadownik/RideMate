using Microsoft.Data.Sqlite;
using RideMate.Api.Development;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RideMate.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using RideMate.Infrastructure.Persistence;
using RideMate.Infrastructure.Security;
using RideMate.Infrastructure.Services;
using RideMate.Api.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// The in-memory database is an explicit, development-only opt-in.
var inMemory = string.Equals(builder.Configuration["Database:Provider"], "InMemory", StringComparison.OrdinalIgnoreCase);
var demoEnabled = builder.Configuration.GetValue<bool>("Demo:Enabled");
if ((inMemory || demoEnabled) && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException("InMemory and demo accounts are available only in Development.");
if (demoEnabled && !inMemory)
{
    var demoDatabase = new Npgsql.NpgsqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection")).Database;
    if (!builder.Configuration.GetValue<bool>("Demo:AllowPersistent") || demoDatabase != "ridemate_demo")
        throw new InvalidOperationException("Persistent demo seeding requires Demo:AllowPersistent=true and the dedicated ridemate_demo database.");
}
if (inMemory)
{
    var memoryConnection = $"Data Source=ridemate-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    builder.Services.AddSingleton<SqliteConnection>(_ => { var keeper = new SqliteConnection(memoryConnection); keeper.Open(); return keeper; });
    builder.Services.AddDbContext<RideMateDbContext>((services, options) => {
        _ = services.GetRequiredService<SqliteConnection>(); // Keep the database alive until the host is disposed.
        options.UseSqlite(memoryConnection);
    });
}
else
    builder.Services.AddDbContext<RideMateDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

if (builder.Configuration["urls"] is null) builder.WebHost.UseUrls("http://0.0.0.0:5050");
if (builder.Environment.IsDevelopment())
    builder.Services.AddCors(options => options.AddPolicy("LocalPreview", policy => policy
        .WithOrigins("http://localhost:8082", "http://127.0.0.1:8082", "http://localhost:8081", "http://127.0.0.1:8081")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key nie jest ustawiony.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RideMate",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RideMateApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// Usługi aplikacji
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IDiscoveryService, DiscoveryService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISafetyService, SafetyService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPushTokenService, PushTokenService>();
builder.Services.AddScoped<IPushNotificationService, NotificationService>();

// HttpClient dla powiadomień push
builder.Services.AddHttpClient();

// SignalR
builder.Services.AddSignalR();

// Polityka admin (w MVP każdy zalogowany — w produkcji dodać rolę)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser());
});

// Kontrolery + Swagger (tylko dev)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RideMate API v1");
    });
}

// Automatyczne migracje przy starcie (MVP, prostota)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RideMateDbContext>();
    // W środowisku developerskim tworzymy bazę automatycznie (brak migracji na etapie 1).
    db.Database.EnsureCreated();
    if (demoEnabled) await DemoSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>(), builder.Configuration);
}

if (app.Environment.IsDevelopment()) app.UseCors("LocalPreview");
if (demoEnabled) app.UseStaticFiles();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", async (RideMateDbContext db) => await db.Database.CanConnectAsync() ? Results.Ok(new { status = "healthy" }) : Results.StatusCode(503));
app.MapHub<ChatHub>("/chatHub");
app.Run();
