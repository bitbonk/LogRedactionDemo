using System.Text.Json;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddHostedService<Worker>()
    .AddLogging(lb =>
    {
        lb.EnableRedaction();

        // Log structured logs as JSON to console so we can see the actual structured data
        lb.AddJsonConsole(o => o.JsonWriterOptions = new JsonWriterOptions { Indented = true });

        // AddRedaction make sure the redactor provider is hooked up so that the logger can get a redactor
        // bey default the ErasingRedactor is added as the fallback redactor which erases all data marked with any
        // DataClassificationAttribute
        // lb.Services.AddRedaction();

        // This is how you can configure redactors in more detail
        lb.Services.AddRedaction(rb =>
            rb.SetRedactor<ErasingRedactor>(
                    new DataClassificationSet(new DataClassification("MyTaxonomy", "MyClassification")))
                .SetFallbackRedactor<NullRedactor>());
    });

var host = builder.Build();
host.Run();

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.UserLoggedIn(new User("abcd", "Charles", "charles.mingus@bluenote.com"));

            await Task.Delay(1000, stoppingToken);
        }
    }
}

public record User(string Id, [PersonalData] string Name, [PersonalData] string Email);

// logging code that logs the user
public static partial class Log
{
    [LoggerMessage(LogLevel.Information, "User logged in")]
    public static partial void UserLoggedIn(this ILogger logger, [LogProperties] User user);
}

public class PersonalDataAttribute : DataClassificationAttribute
{
    // both of those strings are arbitrary identifiers you can pick.
    // You would use them later when configuring redaction to set the policies for your different named classifications.
    public PersonalDataAttribute() : base(new DataClassification("MyTaxonomy", "MyClassification"))
    {
    }
}
