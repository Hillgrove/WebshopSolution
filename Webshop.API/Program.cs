using Webshop.API.Middleware;
using Webshop.Data;
using Webshop.Services;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Application is starting...");

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Connection String
var connectionString = config["Database:ConnectionString"]
    ?? throw new InvalidOperationException("Database connection string is missing from configuration.");

// Add services to the container.
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<HashingService>();
builder.Services.AddHttpClient<PasswordService>();
builder.Services.AddTransient<ValidationService>();
builder.Services.AddSingleton<RateLimitingService>();
//builder.Services.AddSingleton<IUserRepository, UserRepositoryList>();
builder.Services.AddScoped<IUserRepository>(provider => new UserRepositorySQLite(connectionString));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".Webshop.Session";
    options.Cookie.HttpOnly = true;  // Protects against XSS
    options.Cookie.IsEssential = true;  // Ensure GDPR consent doesn't block it
    options.Cookie.SameSite = SameSiteMode.None;  // Required for cross-origin cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Enforce HTTPS
});

// HSTS (ASVS 14.4.5)
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(2*365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// CORS (ASVS 14.5.3)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
                      policy =>
                      {
                          policy.WithOrigins("https://127.0.0.1:5500", "https://localhost:5500", "https://webshop.hillgrove.dk")
                                .WithMethods("GET", "POST", "OPTIONS")
                                .AllowAnyHeader()
                                .AllowCredentials();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Must be before authentication and controllers
app.UseSession();

// Apply CORS policy
app.UseCors("AllowSpecificOrigin");

// Must be before authentication and controllers
app.UseSession();

// Content-Type validation of request headers (ASVS 13.1.5)
app.UseMiddleware<RequestHeaderMiddleware>();

// Ensure Content-Type response header is set properly (ASVS 14.4.1, 14.4.4, 14.4.3)
app.UseMiddleware<ResponseHeaderMiddleware>();

// Custom middleware - Allowed HTTP Methods (ASVS 14.5.1)
app.Use(async (context, next) =>
{
    var allowedMethods = new HashSet<string> { "GET", "POST", "OPTIONS" };
    if (!allowedMethods.Contains(context.Request.Method))
    {
        context.Response.StatusCode = 405;
        await context.Response.WriteAsync("Method Not Allowed");
        return;
    }
    await next();
});

// Apply CSRF protection middleware before authentication
app.UseMiddleware<CsrfMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.Run();
