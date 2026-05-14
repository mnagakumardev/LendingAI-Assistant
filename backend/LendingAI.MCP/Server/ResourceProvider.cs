using LendingAI.MCP.Models;

namespace LendingAI.MCP.Server;

public interface IResourceProvider
{
    Task InitializeAsync();
    MCPResource GetResource(string resourceName);
    List<MCPResource> GetAllResources();
    Task<string> GetResourceContentAsync(string resourceName);
}

public class ResourceProvider : IResourceProvider
{
    private readonly Dictionary<string, MCPResource> _resources = new();
    private readonly ILogger<ResourceProvider> _logger;

    public ResourceProvider(ILogger<ResourceProvider> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Resource Provider...");

        await Task.Run(() =>
        {
            // 1. Loan Products Resource
            _resources["loan-products"] = new MCPResource
            {
                Name = "loan-products",
                Uri = "resource://lending-ai/loan-products",
                Description = "Available loan products and their terms",
                MimeType = "application/json",
                ContentProvider = GetLoanProductsAsync
            };

            // 2. Regulatory Rules Resource
            _resources["regulatory-rules"] = new MCPResource
            {
                Name = "regulatory-rules",
                Uri = "resource://lending-ai/regulatory-rules",
                Description = "Current regulatory rules and limits",
                MimeType = "application/json",
                ContentProvider = GetRegulatoryRulesAsync
            };

            // 3. Risk Thresholds Resource
            _resources["risk-thresholds"] = new MCPResource
            {
                Name = "risk-thresholds",
                Uri = "resource://lending-ai/risk-thresholds",
                Description = "Risk assessment thresholds and scoring parameters",
                MimeType = "application/json",
                ContentProvider = GetRiskThresholdsAsync
            };

            // 4. Fraud Patterns Resource
            _resources["fraud-patterns"] = new MCPResource
            {
                Name = "fraud-patterns",
                Uri = "resource://lending-ai/fraud-patterns",
                Description = "Known fraud patterns and detection rules",
                MimeType = "application/json",
                ContentProvider = GetFraudPatternsAsync
            };

            // 5. Market Rates Resource
            _resources["market-rates"] = new MCPResource
            {
                Name = "market-rates",
                Uri = "resource://lending-ai/market-rates",
                Description = "Current market interest rates by product",
                MimeType = "application/json",
                ContentProvider = GetMarketRatesAsync
            };
        });

        _logger.LogInformation("Resource Provider initialized with {ResourceCount} resources", _resources.Count);
    }

    public MCPResource GetResource(string resourceName)
    {
        if (_resources.TryGetValue(resourceName, out var resource))
        {
            return resource;
        }
        throw new KeyNotFoundException($"Resource '{resourceName}' not found");
    }

    public List<MCPResource> GetAllResources()
    {
        return _resources.Values.ToList();
    }

    public async Task<string> GetResourceContentAsync(string resourceName)
    {
        var resource = GetResource(resourceName);
        return await resource.ContentProvider();
    }

    private async Task<string> GetLoanProductsAsync()
    {
        var products = new[]
        {
            new
            {
                id = "personal-basic",
                name = "Personal Loan - Basic",
                minAmount = 1000m,
                maxAmount = 50000m,
                minRate = 5.99,
                maxRate = 14.99,
                minCreditScore = 300,
                maxTerm = 60
            },
            new
            {
                id = "personal-prime",
                name = "Personal Loan - Prime",
                minAmount = 10000m,
                maxAmount = 100000m,
                minRate = 4.99,
                maxRate = 9.99,
                minCreditScore = 680,
                maxTerm = 84
            },
            new
            {
                id = "home-loan",
                name = "Home Loan",
                minAmount = 50000m,
                maxAmount = 1000000m,
                minRate = 3.5,
                maxRate = 8.5,
                minCreditScore = 620,
                maxTerm = 360
            },
            new
            {
                id = "auto-loan",
                name = "Auto Loan",
                minAmount = 5000m,
                maxAmount = 150000m,
                minRate = 3.99,
                maxRate = 11.99,
                minCreditScore = 500,
                maxTerm = 72
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(products, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> GetRegulatoryRulesAsync()
    {
        var rules = new
        {
            maxDTI = 0.43,
            maxBackEndDTI = 0.50,
            minCreditScore = 300,
            maxInterestRate = 36,
            requireEarningsVerification = true,
            requireAssetVerification = true,
            regulations = new[] { "FCRA", "TILA", "ECOA", "GLBA", "CRA", "SCRA" },
            disclosureRequirements = new
            {
                APR = "Must disclose APR",
                Finance_Charge = "Must disclose finance charge",
                Payment_Schedule = "Must provide payment schedule"
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(rules, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> GetRiskThresholdsAsync()
    {
        var thresholds = new
        {
            lowRisk = new { min = 0, max = 30 },
            mediumRisk = new { min = 31, max = 65 },
            highRisk = new { min = 66, max = 100 },
            scoringFactors = new
            {
                creditScore = 0.35,
                dti = 0.25,
                employmentStability = 0.20,
                incomeVerification = 0.10,
                assetVerification = 0.10
            },
            creditScoreRanges = new
            {
                exceptional = new { min = 800, max = 850 },
                veryGood = new { min = 740, max = 799 },
                good = new { min = 670, max = 739 },
                fair = new { min = 580, max = 669 },
                poor = new { min = 300, max = 579 }
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(thresholds, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> GetFraudPatternsAsync()
    {
        var patterns = new
        {
            highRiskPatterns = new[]
            {
                "Multiple applications in short timeframe",
                "Inconsistent employment history",
                "Unusually high income claims",
                "Frequent address changes",
                "Suspicious document modifications",
                "Identity mismatch across documents"
            },
            documentVerificationRules = new[]
            {
                "Check watermarks and security features",
                "Verify document issue dates",
                "Cross-reference with government databases",
                "Check for document tampering signs"
            ],
            identityVerificationRules = new[]
            {
                "Match photo ID with applicant",
                "Verify SSN with government records",
                "Check for identity theft reports",
                "Verify address history"
            ]
        };

        return System.Text.Json.JsonSerializer.Serialize(patterns, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> GetMarketRatesAsync()
    {
        var rates = new
        {
            lastUpdated = DateTime.UtcNow,
            personalLoans = new
            {
                prime = new { min = 4.99, max = 9.99, avg = 7.49 },
                standard = new { min = 5.99, max = 14.99, avg = 10.49 },
                subprime = new { min = 12.00, max = 29.99, avg = 20.99 }
            },
            homeLoans = new
            {
                thirtyYear = new { min = 3.50, max = 7.50, avg = 5.50 },
                fifteenYear = new { min = 3.00, max = 7.00, avg = 5.00 }
            },
            autoLoans = new
            {
                newCar = new { min = 3.99, max = 11.99, avg = 7.99 },
                usedCar = new { min = 4.99, max = 19.99, avg = 12.49 }
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(rates, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}