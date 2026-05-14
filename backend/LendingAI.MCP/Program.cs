using LendingAI.MCP.Server;
using LendingAI.MCP.Services;
using LendingAI.MCP.Tools;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

// Configure Serilog logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/mcp-server-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("Starting LendingAI MCP Server...");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    // Register MCP Services
    builder.Services.AddSingleton<IMCPServerContext, MCPServerContext>();
    builder.Services.AddSingleton<ILoanAssessmentService, LoanAssessmentService>();
    builder.Services.AddSingleton<ICreditVerificationService, CreditVerificationService>();
    builder.Services.AddSingleton<IFraudDetectionService, FraudDetectionService>();
    builder.Services.AddSingleton<IDocumentVerificationService, DocumentVerificationService>();
    builder.Services.AddSingleton<IMarketAnalysisService, MarketAnalysisService>();
    builder.Services.AddSingleton<IComplianceCheckService, ComplianceCheckService>();
    builder.Services.AddSingleton<IResourceProvider, ResourceProvider>();
    builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();

    // HTTP Client for external APIs
    builder.Services.AddHttpClient();

    // Add Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "LendingAI MCP Server", Version = "v1" });
    });

    var app = builder.Build();

    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LendingAI MCP Server v1"));
    }

    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

    // Initialize tool registry
    var toolRegistry = app.Services.GetRequiredService<IToolRegistry>();
    await toolRegistry.InitializeAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}