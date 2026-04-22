using APIEccomerce.Data;
using APIEccomerce.Models;
using APIEccomerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductoController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR PRODUCTOS
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Listar()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo  && p.Stock >= 1)
                .Select(p => new ProductoResponseDto
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    IdCategoria = p.IdCategoria,
                    CategoriaNombre = p.Categoria.Nombre
                })
                .ToListAsync();

            return Ok(new Response<List<ProductoResponseDto>>
            {
                Value = productos
            });
        }

        // DETALLE
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Obtener(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.IdProducto == id && p.Activo)
                .Select(p => new ProductoResponseDto
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    IdCategoria = p.IdCategoria,
                    CategoriaNombre = p.Categoria.Nombre
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound(new Response<ProductoResponseDto?>
                {
                    Value = null,
                    IsSuccess = false,
                    Message = "Producto no encontrado"
                });
            }

            return Ok(new Response<ProductoResponseDto>
            {
                Value = producto
            });
        }

        //BUSCAR POR NOMBRE
        [HttpGet("buscar")]
        [AllowAnonymous]
        public async Task<IActionResult> Buscar(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new Response<List<ProductoResponseDto>?>
                {
                    Value = null,
                    IsSuccess = false,
                    Message = "El nombre es requerido"
                });
            }

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Nombre.ToLower().Contains(nombre.ToLower()))
                .Select(p => new ProductoResponseDto
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    IdCategoria = p.IdCategoria,
                    CategoriaNombre = p.Categoria.Nombre
                })
                .ToListAsync();

            return Ok(new Response<List<ProductoResponseDto>>
            {
                Value = productos,
                Message = productos.Count == 0 ? "No se encontraron productos" : "OK"
            });
        }

        // POR CATEGORÍA
        [HttpGet("categoria/{idCategoria}")]
        [AllowAnonymous]
        public async Task<IActionResult> PorCategoria(int idCategoria)
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.IdCategoria == idCategoria)
                .Select(p => new ProductoResponseDto
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    IdCategoria = p.IdCategoria,
                    CategoriaNombre = p.Categoria.Nombre
                })
                .ToListAsync();

            return Ok(new Response<List<ProductoResponseDto>>
            {
                Value = productos,
                Message = productos.Count == 0 ? "No hay productos en esta categoría" : "OK"
            });
        }

        // CREAR
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear([FromBody] ProductoDto dto)
        {
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = dto.Stock,
                ImagenUrl = dto.ImagenUrl,
                Activo = dto.Activo,
                IdCategoria = dto.IdCategoria,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(new Response<int>
            {
                Value = producto.IdProducto,
                Message = "Producto creado"
            });
        }

        // ACTUALIZAR
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ProductoDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new Response<string?>
                {
                    Value = null,
                    IsSuccess = false,
                    Message = "Producto no encontrado"
                });
            }

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.Precio = dto.Precio;
            producto.Stock = dto.Stock;
            producto.ImagenUrl = dto.ImagenUrl;
            producto.Activo = dto.Activo;
            producto.IdCategoria = dto.IdCategoria;

            await _context.SaveChangesAsync();

            return Ok(new Response<string>
            {
                Value = "OK",
                Message = "Producto actualizado"
            });
        }

        // ELIMINAR
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new Response<string?>
                {
                    Value = null,
                    IsSuccess = false,
                    Message = "Producto no encontrado"
                });
            }

            producto.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new Response<string>
            {
                Value = "OK",
                Message = "Producto eliminado"
            });
        }
    }
}