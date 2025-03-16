using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
