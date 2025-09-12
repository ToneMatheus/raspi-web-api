using Microsoft.EntityFrameworkCore;
using api_raspi_web.Contexts;
using System;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using api_raspi_web.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserRepo, UserRepo>();

var key = builder.Configuration["Jwt:Key"] ?? "devTony";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "propro";

// JWT auth (validate on protected endpoints)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

/* Add services to the container.
builder.Services.AddControllersWithViews();*/

// API controllers
builder.Services.AddControllers();

// Get the Neon connection string from env
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<RaspidbContext>(options =>
    options.UseNpgsql(connectionString));

// check env var
Console.WriteLine("Using connection string from DATABASE_URL? " + (Environment.GetEnvironmentVariable("DATABASE_URL") != null));

// CORS — allow your frontend(s)
const string CorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "https://tonematheus.netlify.app", // prod
                "http://localhost:5173",                   // Vite dev (adjust if needed)
                "http://localhost:3000"                    // optional: CRA dev
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // .AllowCredentials(); // only if you use cookies/auth; then DO NOT use "*" origins
    });
});

builder.Services.AddControllers();



/* db connection
builder.Services.AddDbContext<RaspidbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("RaspiDbConnection"),
        new MySqlServerVersion(new Version(10, 5)) // adjust for your MariaDB version
    )
);*/


var app = builder.Build();

/* CORS
app.UseCors(policy =>
    policy.WithOrigins("https://tonematheus.netlify.app")
          .AllowAnyMethod()
          .AllowAnyHeader());*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Helpful error body in Production so 500s don't look like CORS issues
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var payload = new { error = "Server error", message = ex?.Message, type = ex?.GetType().FullName };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        });
    });
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

/*app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();*/
app.MapControllers();

/* quick health/debug (optional; remove later)
app.MapGet("/healthz", () => Results.Ok(new { ok = true }));
app.MapGet("/debug/db", async (RaspidbContext db) =>
{
    try { await db.Database.OpenConnectionAsync(); await db.Database.CloseConnectionAsync(); return Results.Ok(new { db = "ok" }); }
    catch (Exception ex) { return Results.Problem(ex.ToString()); }
});*/

app.MapPost("/api/login", async (LoginDto dto, IUserRepo users) =>
{
    var user = await users.FindByUsernameAsync(dto.Username);
    if (user is null) return Results.Unauthorized();

    // Verify against bcrypt hash stored in user.Passwd
    var ok = BCrypt.Net.BCrypt.Verify(dto.Password, user.Passwd);
    if (!ok) return Results.Unauthorized();

    // Claims: use Username since you don’t have Email
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };

    var creds = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(issuer, null, claims,
        expires: DateTime.UtcNow.AddHours(12), signingCredentials: creds);

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = jwt });
});

app.MapGet("/api/me", () => Results.Ok("You are authenticated"))
   .RequireAuthorization();

app.Run();
