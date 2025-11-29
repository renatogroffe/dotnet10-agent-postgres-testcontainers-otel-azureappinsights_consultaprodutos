using Azure.AI.OpenAI;
using Azure.Monitor.OpenTelemetry.Exporter;
using Bogus;
using ConsoleAppChatAIProdutos.Data;
using ConsoleAppChatAIProdutos.Inputs;
using ConsoleAppChatAIProdutos.Plugins;
using ConsoleAppChatAIProdutos.Tracing;
using ConsoleAppChatAIProdutos.Utils;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.ClientModel;
using Testcontainers.PostgreSql;

Console.WriteLine("***** Testes com Agent Framework + Plugins (Functions) + PostgreSQL *****");
Console.WriteLine();

var numberOfRecords = InputHelper.GetNumberOfNewProducts();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

CommandLineHelper.Execute("docker images",
    "Imagens antes da execucao do Testcontainers...");
CommandLineHelper.Execute("docker container ls",
    "Containers antes da execucao do Testcontainers...");

Console.WriteLine("Criando container para uso do PostgreSQL...");
var postgresContainer = new PostgreSqlBuilder()
    .WithImage("postgres:18.1")
    .WithDatabase("basecatalogo")
    .WithResourceMapping(
        DBFileAsByteArray.GetContent("basecatalogo.sql"),
        "/docker-entrypoint-initdb.d/01-basecatalogo.sql")
    .Build();
await postgresContainer.StartAsync();

CommandLineHelper.Execute("docker images",
    "Imagens apos execucao do Testcontainers...");
CommandLineHelper.Execute("docker container ls",
    "Containers apos execucao do Testcontainers...");

var connectionString = postgresContainer.GetConnectionString();
Console.WriteLine($"Connection String da base de dados PostgreSQL: {connectionString}");
CatalogoContext.ConnectionString = connectionString;

var db = new DataConnection(new DataOptions().UsePostgreSQL(connectionString));

var random = new Random();
var fakeProdutos = new Faker<ConsoleAppChatAIProdutos.Data.Fake.Produto>("pt_BR").StrictMode(false)
            .RuleFor(p => p.Nome, f => f.Commerce.Product())
            .RuleFor(p => p.CodigoBarras, f => f.Commerce.Ean13())
            .RuleFor(p => p.Preco, f => random.Next(10, 30))
            .Generate(numberOfRecords);


Console.WriteLine($"Gerando {numberOfRecords} produtos...");
await db.BulkCopyAsync<ConsoleAppChatAIProdutos.Data.Fake.Produto>(fakeProdutos);
Console.WriteLine($"Produtos gerados com sucesso!");
Console.WriteLine();
var resultSelectProdutos = await postgresContainer.ExecScriptAsync(
    "SELECT * FROM \"Produtos\"");
Console.WriteLine(resultSelectProdutos.Stdout);

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(OpenTelemetryExtensions.ServiceName);

var traceProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource(OpenTelemetryExtensions.ServiceName)
    .AddEntityFrameworkCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddAzureMonitorTraceExporter(options =>
    {
        options.ConnectionString = configuration.GetConnectionString("AppInsights");
    })
    .Build();

var agent = new AzureOpenAIClient(endpoint: new Uri(configuration["AzureOpenAI:Endpoint"]!),
        credential: new ApiKeyCredential(configuration["AzureOpenAI:ApiKey"]!))
    .GetChatClient(configuration["AzureOpenAI:DeploymentName"]!)
    .CreateAIAgent(
        instructions: "Você é um assistente de IA que ajuda o usuario a consultar informações" +
            "sobre produtos em uma base de dados do PostgreSQL.",
        tools: [.. ProdutosPlugin.GetFunctions()])
    .AsBuilder()
    .UseOpenTelemetry(sourceName: OpenTelemetryExtensions.ServiceName)
    .Build();


var oldForegroundColor = Console.ForegroundColor;
while (true)
{
    Console.WriteLine("Sua pergunta:");
    var userPrompt = Console.ReadLine();

    using var activity1 = OpenTelemetryExtensions.ActivitySource
        .StartActivity("PerguntaChatIAProdutos")!;

    var result = await agent.RunAsync(userPrompt!);

    Console.WriteLine();
    Console.WriteLine("Resposta da IA:");
    Console.WriteLine();


    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(result.AsChatResponse().Messages.Last().Text);
    Console.ForegroundColor = oldForegroundColor;

    Console.WriteLine();
    Console.WriteLine();

    activity1.Stop();
}