using MpQr.Api.Dtos;

namespace MpQr.Api.Services.Interfaces
{
    public interface IPaymentGateway
    {
        Task<CreatePaymentResponseDto> CreatePaymentAsync(decimal amount, string mode = "web");
        Task<PaymentStatusResponseDto> GetStatusAsync(string externalReference);
        Task<CancelPaymentResponseDto> CancelAsync(string externalReference);
        Task ProcessWebhookAsync(string externalReference, string status, string mercadoPagoPaymentId);
    }
}
