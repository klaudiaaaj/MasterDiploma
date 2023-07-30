using Publisher.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341"));


builder.Services.AddRazorPages();

builder.Services.AddScoped<IRabbitMqSenderDirect, RabbitMqSenderDirect>();
builder.Services.AddScoped<IRabbitMqSenderFanout, RabbitMqSenderFanout>();
builder.Services.AddScoped<IKaffkaSender, KaffkaSender>();
builder.Services.AddScoped<IAzureServiceBusSender, AzureServiceBusSenderQueue>();
builder.Services.AddScoped<IAzureServiceBusSenderTopic, AzureServiceBusSenderTopic>();
builder.Services.AddScoped<IDataProducerService, DataProducerService>();
builder.Services.AddSingleton<ISqLiteRepo, SqLiteRepo>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.MapControllers();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


Log.Information("THERE SERRILOG");