namespace ERP.API.ContractTests;

public class ErpApiContractTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _pactDir;
    private readonly string _pactBrokerUrl;
    private readonly string _apiBaseUrl;

    public ErpApiContractTests(ITestOutputHelper output)
    {
        _output = output;
        _pactDir = Path.Combine(Directory.GetCurrentDirectory(), "pacts");
        _pactBrokerUrl = Environment.GetEnvironmentVariable("PACT_BROKER_URL") ?? "http://localhost:9292";
        _apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";

        // Ensure pact directory exists
        Directory.CreateDirectory(_pactDir);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
