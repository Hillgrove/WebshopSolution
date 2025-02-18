using Webshop.API;
using Webshop.Data;
using Webshop.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Connection String
var connectionString = config["Database:ConnectionString"]
    ?? throw new InvalidOperationException("Database connection string is missing from configuration.");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<HashingService>();
builder.Services.AddHttpClient<PasswordService>();
builder.Services.AddTransient<ValidationService>();
builder.Services.AddSingleton<RateLimitingService>();
//builder.Services.AddSingleton<IUserRepository, UserRepositoryList>();
builder.Services.AddScoped<IUserRepository>(provider => new UserRepositorySQLite(connectionString));

// HSTS (ASVS 14.4.5)
builder.Services.AddHsts(options =>
{
    // TODO: Change MaxAge to 2 years after done testing
    options.MaxAge = TimeSpan.FromMinutes(5);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// CORS (ASVS 14.5.3)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
                      policy =>
                      {
                          policy.WithOrigins("https://127.0.0.1:5500", "https://webshop.hillgrove.dk")
                                .WithMethods("GET", "POST", "OPTIONS")
                                .AllowAnyHeader();
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

// Apply CORS policy
app.UseCors("AllowSpecificOrigin");

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

app.UseAuthorization();
app.MapControllers();
app.Run();
