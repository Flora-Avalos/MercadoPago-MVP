using Microsoft.AspNetCore.Mvc;
using MpQr.Api.Persistence;

namespace MpQr.Api.Controllers
{
    [ApiController]
    [Route("api/store")]
    public class StoreController : ControllerBase
    {
        private readonly StorePaymentRepository _repository;

        public StoreController(StorePaymentRepository repository)
        {
            _repository = repository;
        }

        // 🔎 Devuelve información del pago activo (para POS si lo necesitas)
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePayment()
        {
            var payment = await _repository.GetActiveAsync();

            if (payment == null)
            {
                return Ok(new { hasActivePayment = false });
            }

            return Ok(new
            {
                hasActivePayment = true,
                externalReference = payment.ExternalReference,
                checkoutUrl = payment.CheckoutUrl
            });
        }

        // 🟢 ENDPOINT PARA QR ESTÁTICO (EL IMPORTANTE)
        [HttpGet("scan")]
        public async Task<IActionResult> Scan()
        {
            var payment = await _repository.GetActiveAsync();

            if (payment == null)
            {
                return Content(
                    @"<!DOCTYPE html>
                      <html>
                      <head>
                        <meta charset='UTF-8'>
                        <title>Pago no disponible</title>
                      </head>
                      <body style='font-family:sans-serif;text-align:center;margin-top:50px;'>
                        <h2>No hay un pago habilitado en este momento.</h2>
                        <p>Solicite al cajero que genere el cobro.</p>
                      </body>
                      </html>",
                    "text/html"
                );
            }

            // Redirige al Checkout dinámico actual
            return Redirect(payment.CheckoutUrl);
        }
    }
}