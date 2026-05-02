using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIEccomerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductoController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _service.ListIsActive();
            return Ok(new { value = products });
        }

        [HttpGet("lista")]
        [AllowAnonymous]
        public async Task<IActionResult> Lista()
        {
            var products = await _service.ListIsActive();
            return Ok(new { value = products });
        }

        [HttpGet("categoria")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategoria([FromQuery] string category)
        {
            var products = await _service.ListByCategory(category);
            return Ok(new { value = products });
        }

        [HttpGet("buscar")]
        [AllowAnonymous]
        public async Task<IActionResult> Buscar([FromQuery] string name)
        {
            var products = await _service.SearchByName(name);
            return Ok(new { value = products });
        }

        [HttpGet("public/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByIdPublic(int id)
        {
            var product = await _service.GetByIdPublic(id);
            if (product == null)
                return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(new { value = product });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.GetByIdPublic(id);
            if (product == null)
                return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(new { value = product });
        }

        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAdmin(int id)
        {
            var product = await _service.GetByIdAdmin(id);
            if (product == null)
                return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(new { value = product });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var product = await _service.Create(dto);
            return CreatedAtAction(
                nameof(GetByIdAdmin),
                new { id = product.Id },
                new { value = product }
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _service.Update(id, dto);
            if (product == null)
                return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(new { value = product });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var state = await _service.Delete(id);
            if (!state)
                return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(new { mensaje = "Producto eliminado" });
        }
    }
}