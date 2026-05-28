using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using APIEccomerce.Models;

namespace APIEccomerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _service;

        public AddressController(IAddressService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET api/address
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var addresses = await _service.GetByUserId(GetUserId());
            return Ok(new Response<List<AddressDto>>
            {
                IsSuccess = true,
                Message = "Direcciones obtenidas correctamente",
                Value = addresses
            });
        }

        // GET api/address/default
        [HttpGet("default")]
        public async Task<IActionResult> GetDefault()
        {
            var address = await _service.GetDefaultAddress(GetUserId());
            if (address is null)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message = "No hay dirección predeterminada"
                });

            return Ok(new Response<AddressDto>
            {
                IsSuccess = true,
                Message = "Dirección predeterminada obtenida",
                Value = address
            });
        }

        // POST api/address
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
        {
            var address = await _service.Create(GetUserId(), dto);
            return Ok(new Response<AddressDto>
            {
                IsSuccess = true,
                Message = "Dirección creada correctamente",
                Value = address
            });
        }

        // PUT api/address/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CreateAddressDto dto)
        {
            var address = await _service.Update(GetUserId(), id, dto);
            if (address is null)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Dirección no encontrada"
                });

            return Ok(new Response<AddressDto>
            {
                IsSuccess = true,
                Message = "Dirección actualizada correctamente",
                Value = address
            });
        }

        // DELETE api/address/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(GetUserId(), id);
            if (!result)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Dirección no encontrada"
                });

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = "Dirección eliminada correctamente"
            });
        }
    }
}