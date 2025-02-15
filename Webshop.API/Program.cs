using Webshop.Data;
using Webshop.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Read the connection string from appsettings.json
var connectionString = config["Database:ConnectionString"]
    ?? throw new InvalidOperationException("Database connection string is missing from configuration.");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IUserRepository, UserRepositoryList>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<HashingService>();
builder.Services.AddHttpClient<PasswordService>();
builder.Services.AddTransient<ValidationService>();
builder.Services.AddSingleton<IUserRepository>(provider => new UserRepositorySQLite(connectionString));

// Ratelimiting Options
var rateLimitConfigs = new Dictionary<string, (int MaxAttempts, TimeSpan LockoutDuration)>
{
    ["Login"] = (
        int.Parse(config["RateLimiting:Login:MaxAttempts"]!),
        TimeSpan.FromMinutes(double.Parse(config["RateLimiting:Login:LockoutDurationInMinutes"]!))
    ),
    ["PasswordReset"] = (
        int.Parse(config["RateLimiting:PasswordReset:MaxAttempts"]!),
        TimeSpan.FromMinutes(double.Parse(config["RateLimiting:PasswordReset:LockoutDurationInMinutes"]!))
    )
};

// Ratelimiting
builder.Services.AddSingleton<RateLimitingService>(provider =>
{
    return new RateLimitingService(rateLimitConfigs);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
                      policy =>
                      {
                          policy.WithOrigins("https://127.0.0.1:5500")
                                .AllowAnyMethod()
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

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();
