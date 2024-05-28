using System;
using Jinaga;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marketplace.SaaS.Accelerator.CustomerSite.Integration;

public static class JinagaClientFactory
{
    public static JinagaClient Create(IOptions<JinagaConfiguration> configuration, ILoggerFactory loggerFactory)
    {
        return JinagaClient.Create(options =>
        {
            options.HttpEndpoint = new Uri(configuration.Value.JinagaEndpoint);
            options.HttpAuthenticationProvider = new ServicePrincipalAuthenticationProvider(
                configuration.Value.ServicePrincipalUsername,
                configuration.Value.ServicePrincipalPassword);
            options.LoggerFactory = loggerFactory;
        });
    }
}