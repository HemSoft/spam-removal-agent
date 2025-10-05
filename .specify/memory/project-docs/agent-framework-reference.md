# Microsoft Agent Framework Reference

**Source**: https://github.com/microsoft/agent-framework
**Documentation**: Official Microsoft Agent Framework for .NET
**Last Updated**: 2025-10-05

## Overview

Microsoft Agent Framework is a comprehensive .NET library for building, orchestrating, and deploying AI agents and multi-agent workflows. This document provides reference information for implementing the Spam Removal Agent using the framework.

## Core Concepts

### AIAgent Interface

The `AIAgent` interface is the foundation of all agent implementations:

```csharp
using Microsoft.Agents.AI;

// AIAgent provides:
// - RunAsync() - Execute agent with messages
// - GetNewThread() - Create conversation thread
// - AsAgent() - Convert to agent abstraction
```

### Agent Creation Patterns

#### 1. Chat Client Extension Pattern (Recommended)
```csharp
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;

AIAgent agent = chatClient.CreateAIAgent(
    instructions: "You are a spam detection specialist...",
    name: "spam-detector",
    description: "Autonomous spam detection agent",
    tools: spamDetectionTools
);
```

#### 2. AIAgentBuilder Pattern
```csharp
var builder = new AIAgentBuilder();
builder.Use(/* middleware */);
AIAgent agent = builder.Build(serviceProvider);
```

#### 3. Provider-Specific Patterns
```csharp
// Azure OpenAI
AIAgent azureAgent = new AzureOpenAIClient(endpoint, credential)
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions);

// OpenAI
AIAgent openAIAgent = new OpenAIClient(apiKey)
    .GetChatClient(model)
    .CreateAIAgent(instructions);
```

### Agent Threads

Threads maintain conversation context and history:

```csharp
// Create new thread
AgentThread thread = agent.GetNewThread();

// Run agent with thread
var response = await agent.RunAsync(
    message: "Check inbox for spam",
    thread: thread,
    cancellationToken: cancellationToken
);

// Thread preserves context across multiple runs
var followUp = await agent.RunAsync(
    "Delete the spam you found",
    thread,
    cancellationToken
);
```

### Agent Tools

Tools are functions that agents can call:

```csharp
using Microsoft.Extensions.AI;

// Define tool function
async Task<SpamClassification> DetectSpam(Email email)
{
    // Spam detection logic
    return new SpamClassification { IsSpam = true, Confidence = 0.95 };
}

// Create tool
AITool spamDetectionTool = AIFunctionFactory.Create(DetectSpam);

// Add to agent
AIAgent agent = chatClient.CreateAIAgent(
    instructions: "Use DetectSpam to classify emails",
    tools: new[] { spamDetectionTool }
);
```

## Agent Hosting Patterns

### IHostedService Integration

```csharp
public class SpamRemovalAgentService : BackgroundService
{
    private readonly AIAgent _agent;
    private readonly ILogger _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var thread = _agent.GetNewThread();

        while (!stoppingToken.IsCancellationRequested)
        {
            var emails = await FetchEmailsAsync(stoppingToken);
            if (emails.Any())
            {
                await _agent.RunAsync(
                    $"Analyze {emails.Count} emails",
                    thread,
                    stoppingToken
                );
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

### Dependency Injection Setup

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            // Microsoft Graph client
            services.AddSingleton<IGraphServiceClient>(sp =>
                new GraphServiceClient(new ClientSecretCredential(tenantId, clientId, secret)));

            // Services
            services.AddSingleton<ISpamDetectionService, SpamDetectionService>();

            // AI Chat Client
            services.AddSingleton<IChatClient>(sp =>
                new AzureOpenAIClient(endpoint, credential)
                    .GetChatClient(deploymentName));

            // Agent with tools
            services.AddSingleton<AIAgent>(sp =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();
                var tools = new[] { AIFunctionFactory.Create(DetectSpam) };

                return chatClient.CreateAIAgent(
                    instructions: "Autonomous spam detection with >0.90 confidence threshold",
                    name: "spam-removal-agent",
                    tools: tools
                );
            });

            services.AddHostedService<SpamRemovalAgentService>();
            services.AddApplicationInsightsTelemetry();
        });
```

## Agent Middleware

Middleware wraps agent execution for cross-cutting concerns:

```csharp
public class LoggingAgentMiddleware : DelegatingAIAgent
{
    private readonly ILogger _logger;

    public override async Task<ChatMessage> RunAsync(
        string message, AgentThread thread, CancellationToken ct = default)
    {
        _logger.LogInformation("Processing: {Message}", message);
        var response = await base.RunAsync(message, thread, ct);
        _logger.LogInformation("Completed: {Response}", response);
        return response;
    }
}

// Apply via AIAgentBuilder
var agent = new AIAgentBuilder()
    .Use((inner, sp) => new LoggingAgentMiddleware(inner, sp.GetRequiredService<ILogger>()))
    .Build(serviceProvider);
```

## Multi-Agent Patterns (Future Enhancement)

**Note**: Multi-agent orchestration patterns (sequential workflows, concurrent execution, handoffs, group chat) are available in the framework but not currently implemented. See the [Agent Framework Workflows documentation](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/Workflows) when these patterns are needed.

## Observability Integration

Use OpenTelemetry and Application Insights for monitoring:

```csharp
// Activity source for distributed tracing
private static readonly ActivitySource ActivitySource = new("SpamRemovalAgent", "1.0.0");

// Telemetry tracking
public void TrackSpamDetection(SpamClassification result, Email email)
{
    var telemetry = new EventTelemetry("SpamDetected");
    telemetry.Properties["Subject"] = email.Subject;
    telemetry.Metrics["Confidence"] = result.Confidence;
    _telemetryClient.TrackEvent(telemetry);
}
```

## Best Practices

### Conservative Deletion (≥0.90 confidence required)
```csharp
const double MinimumConfidence = 0.90;
var result = await _agent.RunAsync($"Analyze: {email.Subject}", thread);
return result.IsSpam && result.Confidence >= MinimumConfidence;
```

### Error Recovery with Exponential Backoff
```csharp
for (int attempt = 1; attempt <= 3; attempt++)
{
    try { return await operation(); }
    catch when (attempt < 3)
    {
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }
}
```

### Thread Reset (hourly)
```csharp
private AgentThread GetOrResetThread()
{
    if (_thread == null || DateTime.UtcNow - _threadCreated > TimeSpan.FromHours(1))
        _thread = _agent.GetNewThread();
    return _thread;
}
```

## Package References

```xml
<ItemGroup>
  <!-- Microsoft Agent Framework -->
  <PackageReference Include="Microsoft.Agents.AI" Version="0.1.*" />
  <PackageReference Include="Microsoft.Agents.AI.Workflows" Version="0.1.*" />

  <!-- Microsoft Extensions.AI -->
  <PackageReference Include="Microsoft.Extensions.AI" Version="9.0.*" />
  <PackageReference Include="Microsoft.Extensions.AI.AzureAIInference" Version="9.0.*" />

  <!-- Microsoft Graph -->
  <PackageReference Include="Microsoft.Graph" Version="5.*" />
  <PackageReference Include="Azure.Identity" Version="1.*" />

  <!-- Hosting -->
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.*" />
  <PackageReference Include="Microsoft.Extensions.Logging" Version="8.*" />
  <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.*" />

  <!-- Telemetry -->
  <PackageReference Include="Microsoft.ApplicationInsights" Version="2.*" />
  <PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.*" />
  <PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.*" />
</ItemGroup>
```

## Configuration

```json
{
  "Agent": { "MinimumConfidence": 0.90, "CheckInterval": "00:01:00" },
  "AzureOpenAI": { "Endpoint": "...", "DeploymentName": "gpt-4o-mini" },
  "MicrosoftGraph": { "TenantId": "...", "ClientId": "...", "Scopes": ["Mail.ReadWrite"] },
  "ApplicationInsights": { "ConnectionString": "..." }
}
```

## Additional Resources

- **Main Repository**: https://github.com/microsoft/agent-framework
- **.NET Samples**: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
- **Getting Started**: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted
- **Workflow Patterns**: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/Workflows
- **NuGet Packages**: https://www.nuget.org/packages?q=Microsoft.Agents
