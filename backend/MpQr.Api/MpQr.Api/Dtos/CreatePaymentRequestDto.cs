namespace MpQr.Api.Dtos
{
    public class CreatePaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string? Mode { get; set; }
    }
}
