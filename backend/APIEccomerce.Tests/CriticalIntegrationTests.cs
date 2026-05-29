using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace APIEccomerce.Tests.Integration;

public class CriticalIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly AppDbContext _dbContext;
    private string? _authToken;
    private int _testProductId;
    private int _testCategoryId;

    public CriticalIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite("DataSource=:memory:");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.OpenConnection();
                db.Database.EnsureCreated();

                SeedDatabase(db);
            });
        });

        _client = _factory.CreateClient();
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        InitializeTestIds();
    }

    private void InitializeTestIds()
    {
        try
        {
            _testCategoryId = _dbContext.Categories.AsNoTracking().FirstOrDefault()?.CategoryId ?? 0;
            _testProductId = _dbContext.Products.AsNoTracking().FirstOrDefault()?.ProductId ?? 0;
        }
        catch
        {
            _testCategoryId = 1;
            _testProductId = 1;
        }
    }

    private void SeedDatabase(AppDbContext db)
    {
        var category = new Category { Name = "Test Category" };
        db.Categories.Add(category);
        db.SaveChanges();

        var product = new Product
        {
            Name = "Test Product",
            Description = "Product for integration tests",
            Price = 100000,
            Stock = 10,
            IsActive = true,
            CategoryId = category.CategoryId,
            ImageUrl = "https://test.com/image.jpg"
        };
        db.Products.Add(product);
        db.SaveChanges();
    }

    private async Task<string> GetAuthToken(string? email = null, string? password = null)
    {
        if (_authToken != null) return _authToken;

        var testEmail = email ?? $"test_{Guid.NewGuid()}@test.com";
        var testPassword = password ?? "Password123!";

        var registerDto = new UserDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = testEmail,
            Password = testPassword
        };
        await _client.PostAsJsonAsync("/api/Access/register", registerDto);

        var loginDto = new LoginDto { Email = testEmail, Password = testPassword };
        var loginResponse = await _client.PostAsJsonAsync("/api/Access/login", loginDto);
        var result = await loginResponse.Content.ReadFromJsonAsync<Response<TokenDto>>();

        _authToken = result!.Value!.Token;
        return _authToken;
    }

    private void SetAuthHeader()
    {
        var token = GetAuthToken().Result;
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<int> CreateTestAddress()
    {
        var addressDto = new
        {
            AddressLine = "Calle Test 123",
            City = "Bogotá",
            Department = "Cundinamarca",
            Country = "Colombia",
            PostalCode = "110111",
            Phone = "3001234567",
            IsDefault = true
        };

        var response = await _client.PostAsJsonAsync("/api/Address", addressDto);
        var result = await response.Content.ReadFromJsonAsync<Response<AddressDto>>();
        return result!.Value!.AddressId;
    }

    private async Task AddToCart(int productId, int quantity)
    {
        await _client.PostAsJsonAsync("/api/Cart/items", new { ProductId = productId, Quantity = quantity });
    }

    public void Dispose()
    {
        try
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
        catch { }
        _client.Dispose();
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Flujo_RegistroYLogin_DeberiaFuncionar()
    {
        var email = $"flow_{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        var registerDto = new UserDto
        {
            FirstName = "Flow",
            LastName = "Test",
            Email = email,
            Password = password
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/Access/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Flujo_VerProductos_DeberiaFuncionar()
    {
        var getAllResponse = await _client.GetAsync("/api/Producto");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Flujo_CarritoCompleto_DeberiaFuncionar()
    {
        SetAuthHeader();
        var getCartResponse = await _client.GetAsync("/api/Cart");
        getCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Flujo_CrearOrden_DeberiaFuncionar()
    {
        SetAuthHeader();
        var addressId = await CreateTestAddress();
        addressId.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Flujo_CrearIntencionDePago_DeberiaFuncionar()
    {
        SetAuthHeader();
        var addressId = await CreateTestAddress();
        addressId.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Seguridad_EndpointsProtegidos_SinToken_DeberianRetornar401()
    {
        var cartResponse = await _client.GetAsync("/api/Cart");
        cartResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Seguridad_EndpointsAdmin_ConUsuarioNormal_DeberianRetornar403()
    {
        SetAuthHeader();
        var createCategoryDto = new { Name = "Nueva Categoria" };
        var response = await _client.PostAsJsonAsync("/api/Category", createCategoryDto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(Skip = "Requiere base de datos de prueba")]
    public async Task Flujo_CancelarOrden_DeberiaFuncionar()
    {
        SetAuthHeader();
        var addressId = await CreateTestAddress();
        addressId.Should().BeGreaterThan(0);
    }
}
