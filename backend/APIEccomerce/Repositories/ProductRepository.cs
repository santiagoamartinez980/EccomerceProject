using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // 📦 PRODUCTOS ACTIVOS (CATÁLOGO)
        // =========================
        public async Task<List<Product>> ListIsActive()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<List<Product>> ListByCategory(string category)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive &&
                       p.Category != null &&
                       p.Category.Name.ToLower() == category.ToLower())
                .ToListAsync();
        }

        public async Task<List<Product>> SearchByName(string name)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive &&
                       p.Name.ToLower().Contains(name.ToLower()))
                .ToListAsync();
        }

        // =========================
        // 🟢 GET PUBLICO (solo activos)
        // =========================
        public async Task<Product?> GetByIdPublic(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);
        }

        // =========================
        // 🔐 GET ADMIN (todos los productos)
        // =========================
        public async Task<Product?> GetByIdAdmin(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<List<Product>> getAllAdmin()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        // =========================
        // ➕ CREAR PRODUCTO
        // =========================
        public async Task<Product> Create(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        // =========================
        // ✏️ ACTUALIZAR PRODUCTO
        // (recibe entidad ya modificada desde service)
        // =========================
        public async Task<Product?> Update(Product product)
        {
            var existing = await _context.Products.FindAsync(product.ProductId);

            if (existing == null)
                return null;

            // EF tracking: solo actualizamos campos
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.ImageUrl = product.ImageUrl;
            existing.IsActive = product.IsActive;
            existing.CategoryId = product.CategoryId;

            await _context.SaveChangesAsync();

            return existing;
        }

        // =========================
        // 🗑 DELETE LÓGICO
        // =========================
        public async Task<bool> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return false;

            product.IsActive = false;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}