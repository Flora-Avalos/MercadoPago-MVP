using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MpQr.Api.Dtos;
using MpQr.Api.Persistence;
using MpQr.Api.Services.Interfaces;

namespace MpQr.Api.Services.MercadoPago
{
    public class MercadoPagoCheckoutApiGateway : IPaymentGateway
    {
        private readonly PaymentRepository _repository;
        private readonly IConfiguration _config;

        public MercadoPagoCheckoutApiGateway(
            PaymentRepository repository,
            IConfiguration config)
        {
            _repository = repository;
            _config = config;

            MercadoPagoConfig.AccessToken = config["MercadoPago:AccessToken"];
        }

        public async Task<CreatePaymentResponseDto> CreatePaymentAsync(decimal amount)
        {
            var externalReference = Guid.NewGuid().ToString();

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = "Pago QR",
                        Quantity = 1,
                        CurrencyId = "ARS",
                        UnitPrice = amount
                    }
                },
                ExternalReference = externalReference,
                NotificationUrl = $"{_config["App:BaseUrl"]}/api/payments/webhook",

                // ⏳ EXPIRACIÓN AUTOMÁTICA
                Expires = true,
                ExpirationDateTo = DateTime.UtcNow.AddMinutes(10)
            };

            var client = new PreferenceClient();
            var preference = await client.CreateAsync(request);

            await _repository.InsertAsync(new Models.Payment
            {
                ExternalReference = externalReference,
                Status = "pending",
                Amount = amount
            });

            return new CreatePaymentResponseDto
            {
                ExternalReference = externalReference,
                QrCode = preference.InitPoint, // 🔥 esto es la URL pagable
                Status = "pending"
            };
        }

        public async Task<PaymentStatusResponseDto> GetStatusAsync(string externalReference)
        {
            var status = await _repository.GetStatusAsync(externalReference);

            return new PaymentStatusResponseDto
            {
                Status = status
            };
        }


        public async Task<CancelPaymentResponseDto> CancelAsync(string externalReference)
        {
            await _repository.UpdateStatusAsync(externalReference, "cancelled");

            return new CancelPaymentResponseDto
            {
                Status = "cancelled"
            };
        }

        //bloqueo duro en el gateway
        public async Task ProcessWebhookAsync(
            string externalReference,
            string status,
            string mercadoPagoPaymentId)
        {
            var payment = await _repository.GetByExternalReferenceAsync(externalReference);

            if (payment == null)
                return;

            // 🔒 BLOQUEO DE ESTADOS FINALES
            if (payment.Status == "approved" ||
                payment.Status == "cancelled" ||
                payment.Status == "rejected")
            {
                return; // Payment ya cerrado → ignorar webhook
            }

            // 🔁 IDMPOTENCIA FUERTE
            if (payment.MercadoPagoPaymentId == mercadoPagoPaymentId &&
                payment.Status == status)
            {
                return; // Ya procesado
            }

            await _repository.UpdateStatusAndMpIdAsync(
                externalReference,
                status,
                mercadoPagoPaymentId
            );
        }

    }
}
