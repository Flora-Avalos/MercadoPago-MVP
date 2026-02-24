using Microsoft.Data.SqlClient;
using MpQr.Api.Models;

namespace MpQr.Api.Persistence
{
    public class StorePaymentRepository
    {
        private readonly SqlConnectionFactory _factory;

        public StorePaymentRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        // 🔹 Inserta nuevo pago store
        public async Task InsertAsync(StorePayment payment)
        {
            using var conn = _factory.Create();
            using var cmd = new SqlCommand(@"
                INSERT INTO StorePayments 
                (ExternalReference, Status, Amount, IsEnabled, CheckoutUrl, CreatedAt)
                VALUES (@ref, @status, @amount, @enabled, @checkoutUrl, GETDATE())", conn);

            cmd.Parameters.AddWithValue("@ref", payment.ExternalReference);
            cmd.Parameters.AddWithValue("@status", payment.Status);
            cmd.Parameters.AddWithValue("@amount", payment.Amount);
            cmd.Parameters.AddWithValue("@enabled", payment.IsEnabled);
            cmd.Parameters.AddWithValue("@checkoutUrl", payment.CheckoutUrl);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // 🔹 Marca pago como aprobado y lo deshabilita
        public async Task ApproveAndDisableAsync(string externalReference)
        {
            using var conn = _factory.Create();
            using var cmd = new SqlCommand(@"
                UPDATE StorePayments
                SET Status = 'approved',
                    IsEnabled = 0,
                    UpdatedAt = GETDATE()
                WHERE ExternalReference = @ref", conn);

            cmd.Parameters.AddWithValue("@ref", externalReference);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // 🔹 Solo deshabilita (si alguna vez lo necesitas)
        public async Task DisableAsync(string externalReference)
        {
            using var conn = _factory.Create();
            using var cmd = new SqlCommand(@"
                UPDATE StorePayments
                SET IsEnabled = 0,
                    UpdatedAt = GETDATE()
                WHERE ExternalReference = @ref", conn);

            cmd.Parameters.AddWithValue("@ref", externalReference);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // 🔹 Obtiene el pago activo para QR estático
        public async Task<StorePayment?> GetActiveAsync()
        {
            using var conn = _factory.Create();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 *
                FROM StorePayments
                WHERE Status = 'pending'
                  AND IsEnabled = 1
                ORDER BY CreatedAt DESC", conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
                return null;

            return new StorePayment
            {
                Id = (int)reader["Id"],
                ExternalReference = reader["ExternalReference"].ToString()!,
                Status = reader["Status"].ToString()!,
                Amount = (decimal)reader["Amount"],
                CheckoutUrl = reader["CheckoutUrl"].ToString()!,
                IsEnabled = (bool)reader["IsEnabled"],
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value
                    ? null
                    : (DateTime?)reader["UpdatedAt"]
            };
        }
        public async Task<StorePayment?> GetByExternalReferenceAsync(string externalReference)
        {
            using var conn = _factory.Create();
            using var cmd = new SqlCommand(@"
        SELECT TOP 1 *
        FROM StorePayments
        WHERE ExternalReference = @ref", conn);

            cmd.Parameters.AddWithValue("@ref", externalReference);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
                return null;

            return new StorePayment
            {
                ExternalReference = reader["ExternalReference"].ToString()!,
                Status = reader["Status"].ToString()!,
                Amount = (decimal)reader["Amount"],
                CheckoutUrl = reader["CheckoutUrl"].ToString()!,
                IsEnabled = (bool)reader["IsEnabled"],
                MercadoPagoPaymentId = reader["MercadoPagoPaymentId"] == DBNull.Value
                    ? null
                    : reader["MercadoPagoPaymentId"].ToString()
            };
        }

        public async Task UpdateStatusAndMpIdAsync(
    string externalReference,
    string status,
    string mpId)
        {
            using var conn = _factory.Create();
            using var cmd = new SqlCommand(@"
        UPDATE StorePayments
        SET Status = @status,
            MercadoPagoPaymentId = @mpId,
            IsEnabled = CASE WHEN @status = 'approved' THEN 0 ELSE IsEnabled END,
            UpdatedAt = GETDATE()
        WHERE ExternalReference = @ref", conn);

            cmd.Parameters.AddWithValue("@ref", externalReference);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@mpId", mpId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}