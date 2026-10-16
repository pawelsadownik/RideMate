using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Api.Tests;

public static class TestHost
{
    public static HttpClient CreateClient()
    {
        var factory = new WebApplicationFactory<global::Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("ConnectionStrings__DefaultConnection",
                            "Host=127.0.0.1;Port=5434;Database=ridemate_test;Username=ridemate;Password=ridemate"),
                        new KeyValuePair<string, string?>("Jwt__Key", "test-key-32-chars-long-1234567890"),
                        new KeyValuePair<string, string?>("Jwt__Issuer", "RideMate"),
                        new KeyValuePair<string, string?>("Jwt__Audience", "RideMateApp"),
                        new KeyValuePair<string, string?>("App__DevMode", "true"),
                    });
                });
            });

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RideMateDbContext>();
        db.Database.EnsureCreated();

        return factory.CreateClient();
    }
}
