using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RideMate.Api.Tests;

public static class TestHost
{
    public static HttpClient CreateClient()
    {
        var factory = new WebApplicationFactory<global::Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("Database:Provider", "InMemory");
                builder.UseSetting("Demo:Enabled", "false");
            });
        return factory.CreateClient();
    }
}
