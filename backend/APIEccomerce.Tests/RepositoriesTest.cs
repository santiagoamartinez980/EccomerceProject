using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace APIEccomerce.Tests.Repositories
{
    // ================================================================
    // ProductRepositoryTests
    // ================================================================
    public class ProductRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new ProductRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private async Task<Category> CreateTestCategory()
        {
            var category = new Category { Name = "Test Category" };
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        [Fact]
        public async Task GetByIdPublic_ExistingActiveProduct_ReturnsProduct()
        {
            // Arrange
            var category = await CreateTestCategory();

            var product = new Product
            {
                Name = "Laptop",
                Price = 2500000,
                Stock = 10,
                IsActive = true,
                CategoryId = category.CategoryId
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdPublic(product.ProductId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task GetByIdPublic_ExistingInactiveProduct_ReturnsNull()
        {
            // Arrange
            var category = await CreateTestCategory();

            var product = new Product
            {
                Name = "Discontinued",
                Price = 100,
                Stock = 0,
                IsActive = false,
                CategoryId = category.CategoryId
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdPublic(product.ProductId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdPublic_NonExistingProduct_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdPublic(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ListIsActive_ReturnsOnlyActiveProducts()
        {
            // Arrange
            var category = await CreateTestCategory();

            var products = new List<Product>
            {
                new Product { Name = "Active1", IsActive = true, Price = 100, Stock = 1, CategoryId = category.CategoryId },
                new Product { Name = "Active2", IsActive = true, Price = 200, Stock = 2, CategoryId = category.CategoryId },
                new Product { Name = "Inactive", IsActive = false, Price = 300, Stock = 3, CategoryId = category.CategoryId }
            };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.ListIsActive();

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain(p => p.Name == "Inactive");
        }

        [Fact]
        public async Task ListIsActive_ReturnsEmptyList_WhenNoActiveProducts()
        {
            // Arrange
            var category = await CreateTestCategory();

            var products = new List<Product>
            {
                new Product { Name = "Inactive1", IsActive = false, Price = 100, Stock = 1, CategoryId = category.CategoryId },
                new Product { Name = "Inactive2", IsActive = false, Price = 200, Stock = 2, CategoryId = category.CategoryId }
            };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.ListIsActive();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ListByCategory_ReturnsProductsInCategory()
        {
            // Arrange
            var category = new Category { Name = "Electrónica" };
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            var otherCategory = new Category { Name = "Ropa" };
            _dbContext.Categories.Add(otherCategory);
            await _dbContext.SaveChangesAsync();

            var products = new List<Product>
            {
                new Product { Name = "Laptop", Price = 1000, Stock = 1, IsActive = true, CategoryId = category.CategoryId },
                new Product { Name = "Mouse", Price = 50, Stock = 10, IsActive = true, CategoryId = category.CategoryId },
                new Product { Name = "Camisa", Price = 30, Stock = 5, IsActive = true, CategoryId = otherCategory.CategoryId }
            };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.ListByCategory("Electrónica");

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.Category.Name.Should().Be("Electrónica"));
        }

        [Fact]
        public async Task SearchByName_ReturnsMatchingProducts()
        {
            // Arrange
            var category = await CreateTestCategory();

            var products = new List<Product>
            {
                new Product { Name = "Laptop Gamer", IsActive = true, Price = 1000, Stock = 1, CategoryId = category.CategoryId },
                new Product { Name = "Laptop Office", IsActive = true, Price = 800, Stock = 2, CategoryId = category.CategoryId },
                new Product { Name = "Mouse", IsActive = true, Price = 50, Stock = 10, CategoryId = category.CategoryId }
            };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.SearchByName("Laptop");

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(p => p.Name.Should().Contain("Laptop"));
        }

        [Fact]
        public async Task SearchByName_IsCaseInsensitive()
        {
            // Arrange
            var category = await CreateTestCategory();

            var products = new List<Product>
            {
                new Product { Name = "laptop pro", IsActive = true, Price = 1000, Stock = 1, CategoryId = category.CategoryId },
                new Product { Name = "LAPTOP MAX", IsActive = true, Price = 800, Stock = 2, CategoryId = category.CategoryId }
            };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.SearchByName("laptop");

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchByName_ReturnsEmptyList_WhenNoMatch()
        {
            // Arrange
            var category = await CreateTestCategory();

            var products = new List<Product>
            {
                new Product { Name = "Laptop", IsActive = true, Price = 1000, Stock = 1, CategoryId = category.CategoryId }
            };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.SearchByName("XYZ");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAdmin_ReturnsProduct_WhenExists()
        {
            // Arrange
            var category = await CreateTestCategory();

            var product = new Product
            {
                Name = "Admin Product",
                Price = 100,
                Stock = 5,
                IsActive = true,
                CategoryId = category.CategoryId
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAdmin(product.ProductId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Admin Product");
        }

        [Fact]
        public async Task GetByIdAdmin_ReturnsInactiveProduct_WhenExists()
        {
            // Arrange
            var category = await CreateTestCategory();

            var product = new Product
            {
                Name = "Inactive Admin",
                Price = 100,
                Stock = 0,
                IsActive = false,
                CategoryId = category.CategoryId
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAdmin(product.ProductId);

            // Assert
            result.Should().NotBeNull();
            result!.IsActive.Should().BeFalse();
            result.Name.Should().Be("Inactive Admin");
        }

        [Fact]
        public async Task Create_AddsProductToDatabase()
        {
            // Arrange
            var category = await CreateTestCategory();

            var product = new Product
            {
                Name = "New Product",
                Price = 500,
                Stock = 10,
                IsActive = true,
                CategoryId = category.CategoryId
            };

            // Act
            var result = await _repository.Create(product);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.ProductId.Should().BeGreaterThan(0);
            var saved = await _dbContext.Products.FindAsync(result.ProductId);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("New Product");
        }

        [Fact]
        public async Task Update_ModifiesExistingProduct()
        {
            // Arrange
            var category = await CreateTestCategory();

            var product = new Product
            {
                Name = "Original",
                Price = 100,
                Stock = 5,
                IsActive = true,
                CategoryId = category.CategoryId
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            product.Name = "Updated";
            product.Price = 200;

            // Act
            var result = await _repository.Update(product);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.Name.Should().Be("Updated");
            result.Price.Should().Be(200);
        }

        [Fact]
        public async Task Delete_SoftDeletesProduct()
        {
            // Arrange
            var category = await CreateTestCategory();

            var product = new Product
            {
                Name = "ToDelete",
                Price = 100,
                Stock = 5,
                IsActive = true,
                CategoryId = category.CategoryId
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.Delete(product.ProductId);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.Should().BeTrue();
            var deleted = await _dbContext.Products.FindAsync(product.ProductId);
            deleted!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenProductNotFound()
        {
            // Act
            var result = await _repository.Delete(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAdmin_ReturnsAllProducts()
        {
            // Arrange
            var category = await CreateTestCategory();

            var products = new List<Product>
            {
                new Product { Name = "Active1", IsActive = true, Price = 100, Stock = 1, CategoryId = category.CategoryId },
                new Product { Name = "Active2", IsActive = true, Price = 200, Stock = 2, CategoryId = category.CategoryId },
                new Product { Name = "Inactive", IsActive = false, Price = 300, Stock = 3, CategoryId = category.CategoryId }
            };
            _dbContext.Products.AddRange(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.getAllAdmin();

            // Assert
            result.Should().HaveCount(3);
        }
    }

    // ================================================================
    // CategoryRepositoryTests
    // ================================================================
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly CategoryRepository _repository;

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new CategoryRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task ListCategory_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Electrónica" },
                new Category { Name = "Ropa" }
            };
            _dbContext.Categories.AddRange(categories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.ListCategory();

            // Assert
            result.Should().HaveCount(2);
            result.Select(c => c.Name).Should().Contain(["Electrónica", "Ropa"]);
        }

        [Fact]
        public async Task ListCategory_ReturnsEmptyList_WhenNoCategories()
        {
            // Act
            var result = await _repository.ListCategory();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Create_AddsCategoryToDatabase()
        {
            // Arrange
            var category = new Category { Name = "Nueva" };

            // Act
            var result = await _repository.Create(category);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.CategoryId.Should().BeGreaterThan(0);
            result.Name.Should().Be("Nueva");
        }

        [Fact]
        public async Task Delete_RemovesCategory_WhenNoProducts()
        {
            // Arrange
            var category = new Category { Name = "ToDelete" };
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.Delete(category.CategoryId);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.Should().BeTrue();
            var deleted = await _dbContext.Categories.FindAsync(category.CategoryId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenCategoryHasProducts()
        {
            // Arrange
            var category = new Category { Name = "WithProducts" };
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            var product = new Product
            {
                Name = "Product",
                Price = 100,
                Stock = 1,
                IsActive = true,
                CategoryId = category.CategoryId
            };
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.Delete(category.CategoryId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenCategoryNotFound()
        {
            // Act
            var result = await _repository.Delete(999);

            // Assert
            result.Should().BeFalse();
        }
    }

    // ================================================================
    // AddressRepositoryTests
    // ================================================================
    public class AddressRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly AddressRepository _repository;

        public AddressRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new AddressRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetByUserId_ReturnsUserAddresses()
        {
            // Arrange
            var addresses = new List<Address>
            {
                new Address { UserId = 1, AddressLine = "Calle 1", City = "Bogotá", Country = "Colombia", Phone = "123" },
                new Address { UserId = 1, AddressLine = "Calle 2", City = "Medellín", Country = "Colombia", Phone = "123" },
                new Address { UserId = 2, AddressLine = "Calle 3", City = "Cali", Country = "Colombia", Phone = "123" }
            };
            _dbContext.Addresses.AddRange(addresses);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserId(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(a => a.UserId.Should().Be(1));
        }

        [Fact]
        public async Task GetByUserId_ReturnsEmptyList_WhenNoAddresses()
        {
            // Act
            var result = await _repository.GetByUserId(999);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsAddress_WhenExists()
        {
            // Arrange
            var address = new Address { UserId = 1, AddressLine = "Calle 1", City = "Bogotá", Country = "Colombia", Phone = "123" };
            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetById(address.AddressId);

            // Assert
            result.Should().NotBeNull();
            result!.AddressLine.Should().Be("Calle 1");
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDefaultAddress_ReturnsDefaultAddress_WhenExists()
        {
            // Arrange
            var addresses = new List<Address>
            {
                new Address { UserId = 1, AddressLine = "Default", IsDefault = true, City = "Bogotá", Country = "Colombia", Phone = "123" },
                new Address { UserId = 1, AddressLine = "Other", IsDefault = false, City = "Bogotá", Country = "Colombia", Phone = "123" }
            };
            _dbContext.Addresses.AddRange(addresses);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetDefaultAddress(1);

            // Assert
            result.Should().NotBeNull();
            result!.IsDefault.Should().BeTrue();
            result.AddressLine.Should().Be("Default");
        }

        [Fact]
        public async Task GetDefaultAddress_ReturnsNull_WhenNoDefaultExists()
        {
            // Arrange
            var addresses = new List<Address>
            {
                new Address { UserId = 1, AddressLine = "Other", IsDefault = false, City = "Bogotá", Country = "Colombia", Phone = "123" }
            };
            _dbContext.Addresses.AddRange(addresses);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetDefaultAddress(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_AddsAddressToDatabase()
        {
            // Arrange
            var address = new Address
            {
                UserId = 1,
                AddressLine = "New Address",
                City = "Bogotá",
                Country = "Colombia",
                Phone = "123456789",
                IsDefault = false
            };

            // Act
            var result = await _repository.Create(address);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.AddressId.Should().BeGreaterThan(0);
            var saved = await _dbContext.Addresses.FindAsync(result.AddressId);
            saved.Should().NotBeNull();
            saved!.AddressLine.Should().Be("New Address");
        }

        

        [Fact]
        public async Task Update_ModifiesExistingAddress()
        {
            // Arrange
            var address = new Address
            {
                UserId = 1,
                AddressLine = "Original",
                City = "Bogotá",
                Country = "Colombia",
                Phone = "123",
                IsDefault = false
            };
            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync();

            address.AddressLine = "Updated";

            // Act
            var result = await _repository.Update(address);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.AddressLine.Should().Be("Updated");
        }

        [Fact]
        public async Task Delete_RemovesAddress_WhenExists()
        {
            // Arrange
            var address = new Address
            {
                UserId = 1,
                AddressLine = "ToDelete",
                City = "Bogotá",
                Country = "Colombia",
                Phone = "123"
            };
            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.Delete(address.AddressId);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.Should().BeTrue();
            var deleted = await _dbContext.Addresses.FindAsync(address.AddressId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenAddressNotFound()
        {
            // Act
            var result = await _repository.Delete(999);

            // Assert
            result.Should().BeFalse();
        }
    }

    // ================================================================
    // OrderRepositoryTests
    // ================================================================
    public class OrderRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new OrderRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetByUserId_ReturnsOrdersWithDetailsAndAddress()
        {
            // Arrange
            var address = new Address { UserId = 1, AddressLine = "Calle 1", City = "Bogotá", Country = "Colombia", Phone = "123" };
            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync();

            var order = new Order
            {
                UserId = 1,
                AddressId = address.AddressId,
                Status = OrderStatus.Pending,
                Total = 1000,
                CreatedAt = DateTime.UtcNow,
                Details = new List<OrderDetail>
                {
                    new OrderDetail { ProductId = 1, Quantity = 2, UnitPrice = 500 }
                }
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserId(1);

            // Assert
            result.Should().HaveCount(1);
            result[0].Address.Should().NotBeNull();
            result[0].Details.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByUserId_ReturnsEmptyList_WhenNoOrders()
        {
            // Act
            var result = await _repository.GetByUserId(999);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsOrderWithDetailsAndAddress()
        {
            // Arrange
            var address = new Address { UserId = 1, AddressLine = "Calle 1", City = "Bogotá", Country = "Colombia", Phone = "123" };
            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync();

            var order = new Order
            {
                UserId = 1,
                AddressId = address.AddressId,
                Status = OrderStatus.Pending,
                Total = 1000,
                CreatedAt = DateTime.UtcNow,
                Details = new List<OrderDetail>
                {
                    new OrderDetail { ProductId = 1, Quantity = 2, UnitPrice = 500 }
                }
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetById(order.OrderId);

            // Assert
            result.Should().NotBeNull();
            result!.Address.Should().NotBeNull();
            result.Details.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotFound()
        {
            // Act
            var result = await _repository.GetById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_AddsOrderToDatabase()
        {
            // Arrange
            var address = new Address { UserId = 1, AddressLine = "Calle 1", City = "Bogotá", Country = "Colombia", Phone = "123" };
            _dbContext.Addresses.Add(address);
            await _dbContext.SaveChangesAsync();

            var order = new Order
            {
                UserId = 1,
                AddressId = address.AddressId,
                Status = OrderStatus.Pending,
                Total = 500,
                CreatedAt = DateTime.UtcNow,
                Details = new List<OrderDetail>()
            };

            // Act
            var result = await _repository.Create(order);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.OrderId.Should().BeGreaterThan(0);
            var saved = await _dbContext.Orders.FindAsync(result.OrderId);
            saved.Should().NotBeNull();
            saved!.Total.Should().Be(500);
        }

        [Fact]
        public async Task UpdateStatus_ChangesOrderStatus()
        {
            // Arrange
            var order = new Order
            {
                UserId = 1,
                Status = OrderStatus.Pending,
                Total = 100,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.UpdateStatus(order.OrderId, OrderStatus.Paid);
            await _dbContext.SaveChangesAsync();

            // Assert
            result.Status.Should().Be(OrderStatus.Paid);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNull_WhenOrderNotFound()
        {
            // Act
            var result = await _repository.UpdateStatus(999, OrderStatus.Paid);

            // Assert
            result.Should().BeNull();
        }
    }

    // ================================================================
    // CartRepositoryTests
    // ================================================================
    public class CartRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly CartRepository _repository;

        public CartRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new CartRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetActiveCartByUserIdAsync_ReturnsActiveCart_WhenExists()
        {
            // Arrange
            var cart = new Cart
            {
                UserId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Items = new List<CartItem>()
            };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveCartByUserIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(1);
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetActiveCartByUserIdAsync_ReturnsNull_WhenNoActiveCart()
        {
            // Arrange
            var cart = new Cart
            {
                UserId = 1,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                Items = new List<CartItem>()
            };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveCartByUserIdAsync(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateCartAsync_CreatesNewCart()
        {
            // Act
            var result = await _repository.CreateCartAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(1);
            result.IsActive.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GetCartItemAsync_ReturnsItem_WhenExists()
        {
            // Arrange
            var cart = new Cart { UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 100
            };
            _dbContext.CartItems.Add(cartItem);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetCartItemAsync(cart.Id, 1);

            // Assert
            result.Should().NotBeNull();
            result!.Quantity.Should().Be(2);
        }

        [Fact]
        public async Task GetCartItemAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetCartItemAsync(1, 999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddItemAsync_AddsItemToCart()
        {
            // Arrange
            var cart = new Cart { UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 100
            };

            // Act
            await _repository.AddItemAsync(cartItem);
            await _dbContext.SaveChangesAsync();

            // Assert
            var saved = await _dbContext.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == 1);
            saved.Should().NotBeNull();
            saved!.Quantity.Should().Be(2);
        }

        [Fact]
        public async Task RemoveItemAsync_RemovesItemFromCart()
        {
            // Arrange
            var cart = new Cart { UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 100
            };
            _dbContext.CartItems.Add(cartItem);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.RemoveItemAsync(cartItem);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deleted = await _dbContext.CartItems.FindAsync(cartItem.CartItemId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task ClearCartAsync_RemovesAllItems()
        {
            // Arrange
            var cart = new Cart { UserId = 1, IsActive = true, CreatedAt = DateTime.UtcNow };
            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            var items = new List<CartItem>
            {
                new CartItem { CartId = cart.Id, ProductId = 1, Quantity = 2, UnitPrice = 100 },
                new CartItem { CartId = cart.Id, ProductId = 2, Quantity = 1, UnitPrice = 200 }
            };
            _dbContext.CartItems.AddRange(items);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.ClearCartAsync(cart.Id);
            await _dbContext.SaveChangesAsync();

            // Assert
            var remaining = await _dbContext.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();
            remaining.Should().BeEmpty();
        }
    }
}