using APIEccomerce.Data;
using APIEccomerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIEccomerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context)
        {
            _context = context;
        }

        // 🔍 LISTAR CATEGORÍAS
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Listar()
        {
            var categorias = await _context.Categorias
                .Select(c => new
                {
                    c.IdCategoria,
                    c.Nombre
                })
                .ToListAsync();

            return Ok(new Response<object>
            {
                Value = categorias
            });
        }

        // ➕ CREAR
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Crear([FromBody] Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return Ok(new Response<int>
            {
                Value = categoria.IdCategoria,
                Message = "Categoría creada"
            });
        }
    }
}