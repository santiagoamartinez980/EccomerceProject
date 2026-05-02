using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIEccomerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListCategory()
        {
            var categories = await _service.ListCategory();
            return Ok(new { value = categories });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            var category = await _service.Create(dto);
            return Ok(new { value = category });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var resultado = await _service.Delete(id);
            if (!resultado)
                return NotFound(new { mensaje = "Categoría no encontrada o con productos asociados" });
            return Ok(new { mensaje = "Categoría eliminada" });
        }
    }
}