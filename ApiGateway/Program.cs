using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
ActivitySource ActivitySource = new ActivitySource("ContactQueueService.MessageBroker");
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(
        serviceName: "api-gateway-contact",
        serviceVersion: "1.0.0");
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "api-gateway-contact",
            serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddSource("api-gateway-contact")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://134.122.121.176:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }));

// Configure OpenTelemetry logging
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;

    logging.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://134.122.121.176:4317");
        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
var configBuilder = new ConfigurationBuilder();
configBuilder.AddEnvironmentVariables();
var configuration = configBuilder.Build();

// Obtenha o conteúdo do arquivo ocelot.json
string ocelotJson = File.ReadAllText("ocelot.json");

// Substitua os placeholders pelas variáveis de ambiente
var apiHost = Environment.GetEnvironmentVariable("API_CONTACT_HOST") ?? "localhost";
ocelotJson = ocelotJson.Replace("${API_CONTACT_HOST}", apiHost);

var port = Environment.GetEnvironmentVariable("PORT") ?? "7117";
ocelotJson = ocelotJson.Replace("${PORT}", port);

// Salve o arquivo com as substituições
File.WriteAllText("ocelot.temp.json", ocelotJson);
builder.Configuration.AddJsonFile("ocelot.temp.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();
await app.UseOcelot();

app.Run();
