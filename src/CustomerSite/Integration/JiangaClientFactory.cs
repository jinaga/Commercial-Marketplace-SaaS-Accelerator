using System;
using Jinaga;
using Microsoft.Extensions.Logging;

namespace Marketplace.SaaS.Accelerator.CustomerSite.Integration;

public static class JinagaClientFactory
{
    public static JinagaClient Create(JinagaConfiguration configuration, ILoggerFactory loggerFactory)
    {
        return JinagaClient.Create(options =>
        {
            options.HttpEndpoint = new Uri(configuration.JinagaEndpoint);
            options.HttpAuthenticationProvider = new ServicePrincipalAuthenticationProvider(
                configuration.ServicePrincipalUsername,
                configuration.ServicePrincipalPassword);
            options.LoggerFactory = loggerFactory;
        });
    }
}