using Webshop.Data;
using Webshop.Services;

var builder = WebApplication.CreateBuilder(args);

// Read the connection string from appsettings.json
var connectionString = builder.Configuration["Database:ConnectionString"]
    ?? throw new InvalidOperationException("Database connection string is missing from configuration.");

// Ratelimiting Options
int maxAttempts = int.Parse(builder.Configuration["RateLimiting:MaxAttempts"]
    ?? throw new InvalidOperationException("Rate limiting max attempts is missing from configuration."));
int duration = int.Parse(builder.Configuration["RateLimiting:LockoutDurationInMinutes"]
    ?? throw new InvalidOperationException("Rate limiting lockout duration is missing from configuration."));
TimeSpan lockoutDuration = TimeSpan.FromMinutes(duration);

// Add services to the container.
//builder.Services.AddSingleton<IUserRepository, UserRepositoryList>();
builder.Services.AddSingleton<IUserRepository>(provider => new UserRepositorySQLite(connectionString));
builder.Services.AddTransient<HashingService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<ValidationService>();
builder.Services.AddHttpClient<PwnedPasswordService>();

builder.Services.AddSingleton<RateLimitingService>(provider =>
{
    return new RateLimitingService(maxAttempts, lockoutDuration);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// TODO: add JWT auth?

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
