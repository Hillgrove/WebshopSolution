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
builder.Services.AddSingleton<IUserRepository>(provider => new UserRepositorySQLite(connectionString));
builder.Services.AddTransient<HashingService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<ValidationService>();
builder.Services.AddHttpClient<PwnedPasswordService>();

// Ratelimiting Options
var maxAttempts = int.Parse(config["RateLimiting:MaxAttempts"]!);
var duration = double.Parse(config["RateLimiting:LockoutDurationInMinutes"]!);
var lockoutDuration = TimeSpan.FromMinutes(duration);

// Ratelimiting
builder.Services.AddSingleton<RateLimitingService>(provider =>
{
    return new RateLimitingService(maxAttempts, lockoutDuration);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
                      policy =>
                      {
                          policy.AllowAnyOrigin()
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

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
