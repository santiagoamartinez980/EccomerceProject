using APIEccomerce.Models.DTOs;
using APIEccomerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using APIEccomerce.Models;
using System.Text.Json;

namespace APIEccomerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("intent/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> CreateIntent(int orderId)
        {
            var intent = await _service.CreateIntent(GetUserId(), orderId);
            return Ok(new Response<PaymentIntentDto>
            {
                IsSuccess = true,
                Message   = "Intención de pago creada",
                Value     = intent
            });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var webhook = JsonSerializer.Deserialize<WompiWebhookDto>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (webhook is null)
                return BadRequest();

            await _service.ProcessWebhook(webhook);
            return Ok();
        }
    }
}
