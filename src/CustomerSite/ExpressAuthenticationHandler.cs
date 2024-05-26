using System.Net.Http.Headers;
using System.Threading.Tasks;
using Jinaga.Http;

namespace Marketplace.SaaS.Accelerator.CustomerSite;

public class ExpressAuthenticationHandler : IHttpAuthenticationProvider
{
    public readonly string cookie;

    public ExpressAuthenticationHandler(string cookie)
    {
        this.cookie = cookie;
    }

    public void SetRequestHeaders(HttpRequestHeaders headers)
    {
        headers.Add("connect.sid", cookie);
    }

    public Task<bool> Reauthenticate()
    {
        throw new System.NotImplementedException();
    }
}