using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using RideMate.Api.Development;
using RideMate.Application.Interfaces;
using RideMate.Infrastructure.Persistence;
using Xunit;

namespace RideMate.Api.Tests;

public class DemoTests
{
    private static WebApplicationFactory<global::Program> Factory(string environment = "Development") =>
        new WebApplicationFactory<global::Program>().WithWebHostBuilder(builder => {
            builder.UseEnvironment(environment);
            builder.UseSetting("Database:Provider", "InMemory");
            builder.UseSetting("Demo:Enabled", "true");
        });

    [Fact]
    public async Task SeedsExactlyTwentyConfirmedUsersWithPhotosAndIsIdempotent()
    {
        using var factory = Factory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RideMateDbContext>();
        Assert.Equal(20, await db.Users.CountAsync());
        Assert.Equal(20, await db.Users.CountAsync(u => u.IsEmailConfirmed));
        Assert.Equal(20, await db.ProfilePhotos.CountAsync());
        Assert.Equal(20, await db.Profiles.CountAsync(p => p.Photos.Count == 1));
        await DemoSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>(), scope.ServiceProvider.GetRequiredService<IConfiguration>());
        Assert.Equal(20, await db.Users.CountAsync());
    }

    [Fact]
    public async Task DemoLoginCanDiscoverMatchAndSendMessage()
    {
        using var factory = Factory();
        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "demo01@ridemate.test", password = DemoSeeder.Password });
        login.EnsureSuccessStatusCode();
        var session = await login.Content.ReadFromJsonAsync<Session>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.Token);
        var profiles = await client.GetFromJsonAsync<List<Profile>>("/api/discovery?maxDistanceKm=50");
        Assert.Equal(18, profiles!.Count);
        Assert.All(profiles, p => Assert.Single(p.PhotoUrls));
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RideMateDbContext>();
        var target = await db.Users.SingleAsync(u => u.Email == "demo02@ridemate.test");
        var like = await client.PostAsJsonAsync("/api/discovery/like", new { toUserId = target.Id, like = true });
        like.EnsureSuccessStatusCode();
        Assert.True((await like.Content.ReadFromJsonAsync<LikeResponse>())!.IsMatch);
        var conversations = await client.GetFromJsonAsync<List<Conversation>>("/api/chat/conversations");
        Assert.Equal(2, conversations!.Count);
        var response = await client.PostAsJsonAsync($"/api/chat/conversations/{conversations[0].Id}/messages", new { text = "Manual test message" });
        response.EnsureSuccessStatusCode();
        var messages = await client.GetAsync($"/api/chat/conversations/{conversations[0].Id}/messages");
        messages.EnsureSuccessStatusCode();
    }

    [Fact]
    public void DemoDatabaseCannotStartInProduction()
    {
        using var factory = Factory("Production");
        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }

    private sealed record Session(string Token);
    private sealed record Profile(Guid Id, List<string> PhotoUrls);
    private sealed record LikeResponse(bool IsMatch);
    private sealed record Conversation(Guid Id);
}
