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
        private readonly IOrderService _orderService;

        public PaymentController(IPaymentService service, IOrderService orderService)
        {
            _service = service;
            _orderService = orderService;
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

        /// <summary>
        /// Endpoint de simulación de pago exitoso (para desarrollo)
        /// </summary>
        [HttpPost("confirm/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> ConfirmPayment(int orderId)
        {
            try
            {
                await _orderService.ConfirmPayment(orderId);
                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Pago confirmado y orden completada",
                    Value = new { orderId }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Value = null
                });
            }
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
