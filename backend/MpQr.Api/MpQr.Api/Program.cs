using MpQr.Api.Hubs;
using MpQr.Api.Persistence;
using MpQr.Api.Security;
using MpQr.Api.Services.Interfaces;
using MpQr.Api.Services.MercadoPago;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://mercadopago1.netlify.app"
            )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
              
    });
});

// DI
builder.Services.AddScoped<SqlConnectionFactory>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<IPaymentGateway, MercadoPagoCheckoutApiGateway>();
builder.Services.AddScoped<MercadoPagoSignatureValidator>();
builder.Services.AddScoped<StorePaymentRepository>();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<PaymentHub>("/paymentHub");

app.Run();
