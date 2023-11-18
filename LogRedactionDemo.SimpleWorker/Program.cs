using Microsoft.Extensions.Compliance.Classification;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(lb => lb.EnableRedaction())
    .AddHostedService<Worker>();
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
            {
                _logger.UserLoggedIn(new User("abcd", "Charles", "charles.mingus@bluenote.com"));
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}

public record User(string Id, [PersonalData] string Name, [PersonalData] string Email);

// logging code that logs the user
public static partial class Log
{
    // Error LOGGEN035 : Parameter "user" of logging method "UserLoggedIn" has a sensitive field/property in its type (https://aka.ms/dotnet-extensions-warnings/LOGGEN035)
    // Error CS8795 : Partial method 'Log.UserLoggedIn(ILogger, User)' must have an implementation part because it has accessibility modifiers.
    [LoggerMessage(LogLevel.Information, "User {User} logged in")]
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