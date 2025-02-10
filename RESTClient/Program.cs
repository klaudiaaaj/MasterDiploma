
using RESTClient.cs.Controllers;
using RESTClient.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ClientController>();
builder.Services.AddSingleton<IRosContractor, RosContractor>();

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341"));

var app = builder.Build();

app.MapGet("/", () => "Hello REST CLIENT!");

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
app.MapControllers();

app.MapRazorPages();

app.Run();
