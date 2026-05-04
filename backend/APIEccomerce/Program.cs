using APIEccomerce.Custom;
using APIEccomerce.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using APIEccomerce.Repositories.Interfaces;
using APIEccomerce.Repositories;
using APIEccomerce.Services;
using APIEccomerce.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($">>> Cadena de conexión: {connStr}");

//Dtos Mapping
// Repositorios
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Servicios
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// DbContext con retry
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));

builder.Services.AddScoped<Utilities>();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS
builder.Services.AddCors(opt => opt.AddPolicy("POLITICA_CORS", p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 20-04-20206 Migraciones con reintento manual
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var retries = 5;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("✅ Migraciones aplicadas correctamente");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning($"⏳ Reintentando conexión a BD... ({retries} intentos restantes)");
            if (retries == 0)
                logger.LogError(ex, "❌ No se pudo conectar a la BD");
            else
                Thread.Sleep(5000); // espera 5 segundos antes de reintentar
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("POLITICA_CORS");
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();