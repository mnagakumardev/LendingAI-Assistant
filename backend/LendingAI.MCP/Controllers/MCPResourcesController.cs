using LendingAI.MCP.Models;
using LendingAI.MCP.Server;
using Microsoft.AspNetCore.Mvc;

namespace LendingAI.MCP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MCPResourcesController : ControllerBase
{
    private readonly IResourceProvider _resourceProvider;
    private readonly ILogger<MCPResourcesController> _logger;

    public MCPResourcesController(
        IResourceProvider resourceProvider,
        ILogger<MCPResourcesController> logger)
    {
        _resourceProvider = resourceProvider;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<List<MCPResource>> GetAllResources()
    {
        var resources = _resourceProvider.GetAllResources();
        return Ok(resources.Select(r => new { r.Name, r.Uri, r.Description, r.MimeType }).ToList());
    }

    [HttpGet("{resourceName}")]
    public async Task<ActionResult<string>> GetResource(string resourceName)
    {
        _logger.LogInformation("Fetching resource: {ResourceName}", resourceName);

        try
        {
            var content = await _resourceProvider.GetResourceContentAsync(resourceName);
            return Ok(content);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Resource not found: {ResourceName}", resourceName);
            return NotFound(new { error = $"Resource '{resourceName}' not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching resource: {ResourceName}", resourceName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("loan-products")]
    public async Task<ActionResult<string>> GetLoanProducts()
    {
        return await GetResource("loan-products");
    }

    [HttpGet("regulatory-rules")]
    public async Task<ActionResult<string>> GetRegulatoryRules()
    {
        return await GetResource("regulatory-rules");
    }

    [HttpGet("risk-thresholds")]
    public async Task<ActionResult<string>> GetRiskThresholds()
    {
        return await GetResource("risk-thresholds");
    }

    [HttpGet("fraud-patterns")]
    public async Task<ActionResult<string>> GetFraudPatterns()
    {
        return await GetResource("fraud-patterns");
    }

    [HttpGet("market-rates")]
    public async Task<ActionResult<string>> GetMarketRates()
    {
        return await GetResource("market-rates");
    }
}