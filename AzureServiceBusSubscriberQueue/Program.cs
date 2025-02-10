using AzureServiceBusSubscriber.Services;
using AzureServiceBusSubscriberQueue.Services;
using RabbitmqSubscriber.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IAzureServiceBusGetConnection, AzureServiceBusGetConnection>();
builder.Services.AddSingleton<IRosContractor, RosContractor>();
//ilder.Services.AddHostedService<AzureServiceBusSubscriberService>();
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341"));
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
