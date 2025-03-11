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
//builder.Services.AddSingleton<IProductRepository, ProductRepositoryList>();
builder.Services.AddScoped<IUserRepository>(provider => new UserRepositorySQLite(connectionString));
builder.Services.AddScoped<IProductRepository>(provider => new ProductRepositorySQLite(connectionString));

// ASVS: 3.2.3 - Store session tokens securely using HttpOnly and Secure cookies
builder.Services.AddSession(options =>
{
    // ASVS: 3.4.1 - Enforce Secure attribute to ensure session cookies are only sent over HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Prevents transmission over HTTP

    // ASVS: 3.4.2 - Enforce HttpOnly to prevent JavaScript access and mitigate XSS risks
    options.Cookie.HttpOnly = true;  // Blocks client-side scripts from accessing session cookies

    // ASVS: 3.4.3 - Use the SameSite attribute to mitigate CSRF attacks
    options.Cookie.SameSite = SameSiteMode.None;  // Required for cross-site authentication

    // ASVS: 3.4.4 - Use "__Host-" prefix to enforce HTTPS and prevent domain-wide cookies
    options.Cookie.Name = "__Host-WebshopSession";  // Strengthens cookie scoping

    // TODO: Fix this - if set to "/api" sessions cookies are not created in browser
    // ASVS: 3.4.5 - Restrict session cookie scope to API routes only
    options.Cookie.Path = "/";  // Ensures session cookies are only sent to API endpoints

    // Ensures the session cookie is always available, even if consent is not given
    options.Cookie.IsEssential = true;

    // Defines session expiration policies
    options.Cookie.MaxAge = TimeSpan.FromHours(1);  // Allows session persistence even after browser restart
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Automatically expires session after inactivity
});


// ASVS: 14.4.5 - Verify that a Strict-Transport-Security (HSTS) header  is included on all responses
//                and for all subdomains
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(2*365); // Recommended time is 2 years
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// CORS
// ASVS: 14.5.3
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

// Initialize databases asynchronously
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
    var productRepo = serviceProvider.GetRequiredService<IProductRepository>();

    if (userRepo is UserRepositorySQLite userRepositorySQLite)
    {
        await userRepositorySQLite.InitializeDatabase();
    }

    if (productRepo is ProductRepositorySQLite productRepositorySQLite)
    {
        await productRepositorySQLite.InitializeDatabase();
    }
}

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

// ASVS: 13.2.3 - Protect RESTful services from CSRF using origin request headers
app.UseMiddleware<OriginValidationMiddleware>();

// Content-Type validation of request headers (ASVS 13.1.5)
app.UseMiddleware<RequestHeaderMiddleware>();

// ASVS: 13.2.3 - Protect RESTful services from CSRF using the Double Submit Cookie Pattern
//app.UseMiddleware<CsrfMiddleware>();


// ASVS: 14.4.1 - Ensure Content-Type is Set Correctly
// ASVS: 14.4.3 - Enforce Content-Security-Policy headers (CSP)
// ASVS: 14.4.4 - Ensure nosniff header on all responses
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

app.UseAuthorization();
app.MapControllers();
app.Run();
