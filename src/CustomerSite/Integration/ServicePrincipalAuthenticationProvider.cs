using System.Net.Http.Headers;
using System.Threading.Tasks;
using Jinaga.Http;

namespace Marketplace.SaaS.Accelerator.CustomerSite.Integration;

public class ServicePrincipalAuthenticationProvider : IHttpAuthenticationProvider
{
    private readonly string username;
    private readonly string password;

    public ServicePrincipalAuthenticationProvider(string username, string password)
    {
        this.username = username;
        this.password = password;
    }

    public void SetRequestHeaders(HttpRequestHeaders headers)
    {
        var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
        headers.Authorization = new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(byteArray));
    }

    public Task<bool> Reauthenticate()
    {
        return Task.FromResult(false);
    }
}