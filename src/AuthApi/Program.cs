using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<LogClient>(client =>
{
    var url = builder.Configuration["LoggingService:BaseUrl"]
        ?? "http://localhost:5003";

    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
});
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("AuthDb")
    ?? throw new InvalidOperationException("ConnectionString AuthDb fehlt.");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    db.Database.Migrate();

    var hasher = scope.ServiceProvider
    .GetRequiredService<IPasswordHasher<User>>();

    var testUsers = new[]
    {
    ("testuser1", 1, builder.Configuration["SeedUsers:TestUser1Password"]),
    ("testuser2", 2, builder.Configuration["SeedUsers:TestUser2Password"]),
    ("testuser3", 3, builder.Configuration["SeedUsers:TestUser3Password"])
};

    foreach (var (username, level, password) in testUsers)
    {
        if (string.IsNullOrWhiteSpace(password))
            continue;

        if (await db.Users.AnyAsync(u => u.Username == username))
            continue;

        var user = new User
        {
            Username = username,
            Role = UserRole.User,
            SupportLevel = level,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
    }

    await db.SaveChangesAsync();


}

app.Run();


