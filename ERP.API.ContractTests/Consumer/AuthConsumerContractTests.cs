using FluentAssertions;
using PactNet;
using PactNet.Matchers;
using Xunit;
using Xunit.Abstractions;

namespace ERP.API.ContractTests.Consumer;

/// <summary>
/// Consumer-driven contract tests for ERP API.
/// These tests define the expected behavior from the frontend consumer perspective.
/// Run these tests to generate pact files that can be published to a Pact Broker.
/// </summary>
public class AuthConsumerContractTests : IDisposable
{
    private readonly IPactBuilder _pact;
    private readonly string _pactDir;
    private readonly ITestOutputHelper _output;

    public AuthConsumerContractTests(ITestOutputHelper output)
    {
        _output = output;
        _pactDir = Path.Combine(Directory.GetCurrentDirectory(), "pacts");

        var config = new PactConfig
        {
            PactDir = _pactDir,
            DefaultJsonSettings = new JsonSerializerSettings
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            },
            LogLevel = PactLogLevel.Debug
        };

        _pact = new PactBuilder(config)
            .ServiceConsumer("NEXTERP.Frontend")
            .HasPactWith("ERP.API");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var expectedResponse = new
        {
            success = true,
            token = Match.Type("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."),
            user = new
            {
                id = Match.Type("usr_123"),
                email = "admin@nexterp.com",
                name = Match.Type("Admin User"),
                role = Match.Type("Admin")
            },
            expiresAt = Match.Type(DateTime.UtcNow.AddHours(1))
        };

        _pact
            .UponReceiving("a valid login request with correct credentials")
                .Given("a user with valid credentials exists")
                .WithRequest(HttpMethod.Post, "/api/auth/login")
                .WithHeaders(new
                {
                    Content-Type = "application/json",
                    Accept = "application/json"
                })
                .WithBody(new
                {
                    email = "admin@nexterp.com",
                    password = "Admin@123!"
                })
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeaders(new
                {
                    Content_Type = Match.Regex("application/json.*", "application/json")
                })
                .WithBody(expectedResponse);

        await _pact.VerifyAsync(async ctx =>
        {
            var response = await ctx.MakeRequest(new
            {
                Email = "admin@nexterp.com",
                Password = "Admin@123!"
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        _pact
            .UponReceiving("a login request with invalid credentials")
                .Given("no special state")
                .WithRequest(HttpMethod.Post, "/api/auth/login")
                .WithHeaders(new
                {
                    Content_Type = "application/json"
                })
                .WithBody(new
                {
                    email = "wrong@test.com",
                    password = "wrongpassword"
                })
            .WillRespond()
                .WithStatus(HttpStatusCode.Unauthorized)
                .WithBody(new
                {
                    success = false,
                    error = Match.Type("Invalid email or password"),
                    correlationId = Match.Type("corr_123")
                });

        await _pact.VerifyAsync(async ctx =>
        {
            var response = await ctx.MakeRequest(new
            {
                Email = "wrong@test.com",
                Password = "wrongpassword"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        });
    }

    [Fact]
    public async Task Login_WithMissingFields_ReturnsBadRequest()
    {
        _pact
            .UponReceiving("a login request with missing fields")
                .Given("no special state")
                .WithRequest(HttpMethod.Post, "/api/auth/login")
                .WithHeaders(new
                {
                    Content_Type = "application/json"
                })
                .WithBody(new
                {
                    email = "",
                    password = ""
                })
            .WillRespond()
                .WithStatus(HttpStatusCode.BadRequest)
                .WithBody(new
                {
                    success = false,
                    error = Match.Regex(".*(email|password).*", "Validation failed"),
                    correlationId = Match.Type("corr_123")
                });

        await _pact.VerifyAsync(async ctx =>
        {
            var response = await ctx.MakeRequest(new
            {
                Email = "",
                Password = ""
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    public void Dispose()
    {
        _pact.Build();
        _output.WriteLine($"Pact file written to: {_pactDir}");
    }
}

public class HealthConsumerContractTests : IDisposable
{
    private readonly IPactBuilder _pact;
    private readonly string _pactDir;

    public HealthConsumerContractTests(ITestOutputHelper output)
    {
        _pactDir = Path.Combine(Directory.GetCurrentDirectory(), "pacts");

        var config = new PactConfig
        {
            PactDir = _pactDir,
            DefaultJsonSettings = new JsonSerializerSettings
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            },
            LogLevel = PactLogLevel.Debug
        };

        _pact = new PactBuilder(config)
            .ServiceConsumer("NEXTERP.Frontend")
            .HasPactWith("ERP.API");
    }

    [Fact]
    public async Task HealthLive_ReturnsHealthy()
    {
        _pact
            .UponReceiving("a liveness check request")
                .Given("the API is running")
                .WithRequest(HttpMethod.Get, "/health/live")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeaders(new
                {
                    Content_Type = Match.Regex("application/json.*", "application/json")
                })
                .WithBody(new
                {
                    status = Match.Regex("Healthy|alive", "Healthy"),
                    timestamp = Match.Type(DateTime.UtcNow)
                });

        await _pact.VerifyAsync(async ctx =>
        {
            var response = await ctx.MakeRequest();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task HealthReady_WhenDependenciesHealthy_ReturnsReady()
    {
        _pact
            .UponReceiving("a readiness check request when all dependencies are healthy")
                .Given("PostgreSQL and Redis are accessible")
                .WithRequest(HttpMethod.Get, "/health/ready")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithBody(new
                {
                    status = Match.Type("Healthy"),
                    timestamp = Match.Type(DateTime.UtcNow),
                    totalDuration = Match.Range(0, 5000),
                    checks = Match.Each(new
                    {
                        name = Match.Type("postgresql"),
                        status = Match.Type("Healthy"),
                        duration = Match.Range(0, 5000)
                    })
                });

        await _pact.VerifyAsync(async ctx =>
        {
            var response = await ctx.MakeRequest();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        });
    }

    public void Dispose()
    {
        _pact.Build();
    }
}
