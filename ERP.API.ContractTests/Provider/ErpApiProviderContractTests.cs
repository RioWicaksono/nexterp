using FluentAssertions;
using PactNet.Infrastructure.Outputters;
using PactNet.Verifier;
using Xunit;
using Xunit.Abstractions;

namespace ERP.API.ContractTests.Provider;

/// <summary>
/// Provider contract tests for ERP API.
/// These tests verify that the API matches the contract defined by the consumer.
/// Run against a running API instance.
/// </summary>
public class ErpApiProviderContractTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _pactDir;
    private readonly string _pactBrokerUrl;
    private readonly string _apiBaseUrl;

    public ErpApiProviderContractTests(ITestOutputHelper output)
    {
        _output = output;
        _pactDir = Path.Combine(Directory.GetCurrentDirectory(), "pacts");
        _pactBrokerUrl = Environment.GetEnvironmentVariable("PACT_BROKER_URL") ?? "http://localhost:9292";
        _apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";
    }

    [Fact]
    public void VerifyProvider_WithAuthPact_MeetsContract()
    {
        // This test verifies the API against all pacts involving ERP.API as provider
        // for the NEXTERP.Frontend consumer

        var pactFile = Path.Combine(_pactDir, "nexterp-frontend-erp-api.json");

        if (!File.Exists(pactFile))
        {
            _output.WriteLine($"Pact file not found at {pactFile}. Run consumer tests first.");
            return;
        }

        var verifier = new PactVerifier(new PactVerifierConfig
        {
            LogLevel = PactNet.Infrastructure.Logging.PactLogLevel.Debug,
            Outputters = new List<IOutput>
            {
                new XUnitOutput(_output)
            },
            PublishVerificationResults = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PACT_BROKER_URL")),
            ProviderVersion = GetBuildVersion()
        });

        verifier
            .ServiceProvider("ERP.API")
            .WithFileSource(new FileInfo(pactFile))
            .WithApiEndpoint(_apiBaseUrl)
            .WithRequestTimeout(TimeSpan.FromSeconds(30))
            .Verify();
    }

    [Fact]
    public void VerifyProvider_FromBroker_MeetsContract()
    {
        // This test verifies against pacts published to a Pact Broker
        // Configure PACT_BROKER_URL environment variable to enable

        if (string.IsNullOrEmpty(_pactBrokerUrl))
        {
            _output.WriteLine("PACT_BROKER_URL not configured. Skipping broker verification.");
            return;
        }

        var verifier = new PactVerifier(new PactVerifierConfig
        {
            LogLevel = PactNet.Infrastructure.Logging.PactLogLevel.Debug,
            Outputters = new List<IOutput>
            {
                new XUnitOutput(_output)
            },
            PublishVerificationResults = true,
            ProviderVersion = GetBuildVersion()
        });

        verifier
            .ServiceProvider("ERP.API")
            .WithPactBrokerSource(new Uri(_pactBrokerUrl), options =>
            {
                options.ConsumerVersionSelectors(
                    new PactNet.Infrastructure.DataSource.Pacts.FlatFile.ConsumerVersionSelector
                    {
                        Latest = true
                    },
                    new PactNet.Infrastructure.DataSource.Pacts.FlatFile.ConsumerVersionSelector
                    {
                        Consumer = "NEXTERP.Frontend",
                        Latest = true
                    }
                );
                options.ConsumerVersionTags("main", "master");
            })
            .WithApiEndpoint(_apiBaseUrl)
            .WithRequestTimeout(TimeSpan.FromSeconds(30))
            .Verify();
    }

    private static string GetBuildVersion()
    {
        var version = Environment.GetEnvironmentVariable("BUILD_NUMBER")
            ?? Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER")
            ?? "local";

        return $"1.0.0-{version}";
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
