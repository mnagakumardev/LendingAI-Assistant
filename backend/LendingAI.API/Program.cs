using LendingAI.Infrastructure;
using LendingAI.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Cosmos.Fluent;
using Azure.AI.OpenAI;
using LendingAI.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Services
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddAzureClients(x =>
{
    x.AddOpenAIClient(new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!))
        .WithCredential(new Azure.Identity.DefaultAzureCredential());
});

// Add services
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
builder.Services.AddScoped<IAIAgentService, AIAgentService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<ICreditScoringService, CreditScoringService>();
builder.Services.AddSingleton(sp =>
{
    var client = new CosmosClientBuilder(builder.Configuration["CosmosDB:ConnectionString"]!)
        .WithConnectionModeDirect()
        .Build();
    return client.GetDatabase(builder.Configuration["CosmosDB:DatabaseName"]!);
});

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReact");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();