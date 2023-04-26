using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EventSourcingCore.Tests.Implementations
{
    public class TestWebApplicationFactory<T> : WebApplicationFactory<T>
        where T : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder().UseStartup<T>();
        }
    }
}
