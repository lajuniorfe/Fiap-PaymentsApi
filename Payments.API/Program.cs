using Payments.API.Consumers;
using Payments.API.Messaging;
using Payments.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMessageBus, RabbitMqMessageBus>(); 
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddHostedService<OrderPlacedConsumer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
