using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Metrics;
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
    .WithMetrics(metrics =>
    {
        metrics
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter()
            .AddAspNetCoreInstrumentation();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
var configBuilder = new ConfigurationBuilder();
configBuilder.AddEnvironmentVariables();
var configuration = configBuilder.Build();

// Obtenha o conte�do do arquivo ocelot.json
string ocelotJson = File.ReadAllText("ocelot.json");

// Substitua os placeholders pelas vari�veis de ambiente
var apiHost = Environment.GetEnvironmentVariable("API_CONTACT_HOST") ?? "localhost";
ocelotJson = ocelotJson.Replace("${API_CONTACT_HOST}", apiHost);

var port = Environment.GetEnvironmentVariable("PORT") ?? "7117";
ocelotJson = ocelotJson.Replace("${PORT}", port);

var codeAll = Environment.GetEnvironmentVariable("codeAll");
ocelotJson = ocelotJson.Replace("${codeAll}", codeAll);

var codeById = Environment.GetEnvironmentVariable("codeById");
ocelotJson = ocelotJson.Replace("${codeById}", codeById);


var url = Environment.GetEnvironmentVariable("ApiContactsUrl");
ocelotJson = ocelotJson.Replace("${ApiContactsUrl}", url);

// Salve o arquivo com as substitui��es
File.WriteAllText("ocelot.temp.json", ocelotJson);
builder.Configuration.AddJsonFile("ocelot.temp.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapControllers();
await app.UseOcelot();

app.Run();
