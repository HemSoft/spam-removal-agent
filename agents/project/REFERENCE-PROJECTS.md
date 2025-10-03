---
title: "Reference Projects Guide"
version: "1.0.1"
lastModified: "2025-09-30"
author: "Franz Hemmer"
purpose: "Overview of reference projects in /ref and how to leverage them for implementation patterns."
---

# Reference Projects - Instruction for using the ./ref reference projects effectively.

## Reference Projects

In the `/ref` folder you will find C# projects that each work and act as a standard to follow in terms of implementation. These are proof-of-concept (POC) implementations that demonstrate integration patterns used in the main Relias Assistant application.

### Integration POCs

- **ChatClient-AI-POC** - Demonstrates Azure OpenAI chat client integration using Microsoft.Extensions.AI. Shows how to configure and interact with Azure OpenAI services, including chat completions and function calling patterns.

- **Conflurnce-POC** - Shows how to search for Confluence documents. Pay attention to the NuGet packages used here. Demonstrates authentication and document retrieval from Atlassian Confluence.

- **GitHub-Octokit-POC** - Demonstrates GitHub integration using the Octokit library. Shows repository search, code search, and GitHub API interaction patterns.

- **JIRA-POC** - Demonstrates JIRA integration for ticket retrieval and search. Shows authentication and query patterns for Atlassian JIRA.

- **VectorStoreSearch-POC** - Excellent example of how to work with Azure AI Search using a vector store to search proprietary Relias information such as the Relias Employee Handbook and onboarding markdown files. Demonstrates embedding-based semantic search patterns.

### Utility Projects

- **configurator** - Utility tool for managing configuration settings. Useful for understanding configuration patterns and secret management.

- **pe-assistant** - Predecessor to Relias Assistant (Yakob). Contains earlier implementation patterns that may provide historical context for architectural decisions.

## How to Use These Projects

1. **Integration Patterns** - Reference these POCs when implementing or extending integrations in the main application. The service implementations in `Relias.Assistant.AI` are based on these patterns.

2. **NuGet Packages** - Check the `.csproj` files in these POCs to identify the correct NuGet packages and versions for specific integrations.

3. **Authentication** - Review how each POC handles authentication, API keys, and configuration management.

4. **Error Handling** - Observe error handling patterns and how external service failures are managed gracefully.

5. **Testing Approaches** - Some POCs may include test patterns useful for validating integrations.

## Best Practices

- Use these projects as **reference implementations**, not as code to copy directly
- Adapt patterns to fit the architecture of the main application
- Pay attention to configuration and secret management approaches
- Consider the resilience and error handling patterns demonstrated
- Update this document if new POC projects are added to the `/ref` directory
