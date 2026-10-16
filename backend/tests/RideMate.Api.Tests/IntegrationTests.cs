using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RideMate.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RideMate.Api.Tests;

public class IntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _client = TestHost.CreateClient();
    private readonly string _emailA = $"a{Guid.NewGuid():N}@example.com";
    private readonly string _emailB = $"b{Guid.NewGuid():N}@example.com";
    private string? _tokenA;
    private string? _tokenB;
    private Guid _userIdA;
    private Guid _userIdB;

    public async Task InitializeAsync()
    {
        var regA = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = _emailA,
            password = "Haslo1234!",
            dateOfBirth = "1990-01-01"
        });
        if (!regA.IsSuccessStatusCode)
        {
            var body = await regA.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Register A failed: {regA.StatusCode} {body}");
        }
        var dataA = await regA.Content.ReadFromJsonAsync<AuthResponseDto>();
        _tokenA = dataA!.Token;
        _userIdA = dataA.UserId;

        var regB = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = _emailB,
            password = "Haslo1234!",
            dateOfBirth = "1988-05-05"
        });
        regB.EnsureSuccessStatusCode();
        var dataB = await regB.Content.ReadFromJsonAsync<AuthResponseDto>();
        _tokenB = dataB!.Token;
        _userIdB = dataB.UserId;

        await SetProfile(_tokenA, _userIdA, "Marek", 30, 52.23, 21.01);
        await SetProfile(_tokenB, _userIdB, "Anna", 28, 50.06, 19.94);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SetProfile(string? token, Guid userId, string nickname, int age, double lat, double lon)
    {
        SetToken(token);
        var res = await _client.PutAsJsonAsync("/api/profiles/me", new
        {
            nickname,
            age,
            lookingFor = "jointRides",
            activityType = "motorcycle",
            ridingStyle = "Touring",
            bio = "Test bio",
            city = "Warszawa",
            latitude = lat,
            longitude = lon
        });
        res.EnsureSuccessStatusCode();
    }

    private void SetToken(string? token)
    {
        _client.DefaultRequestHeaders.Remove("Authorization");
        if (token is not null)
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        SetToken(null);
        var res = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = _emailA,
            password = "Haslo1234!",
            dateOfBirth = "1990-01-01"
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Like_ThenMutualLike_CreatesMatch()
    {
        SetToken(_tokenA);
        var likeRes = await _client.PostAsJsonAsync("/api/discovery/like", new { toUserId = _userIdB, like = true });
        likeRes.EnsureSuccessStatusCode();
        var likeResult = await likeRes.Content.ReadFromJsonAsync<LikeResultDto>();
        Assert.False(likeResult!.IsMatch);

        SetToken(_tokenB);
        var likeRes2 = await _client.PostAsJsonAsync("/api/discovery/like", new { toUserId = _userIdA, like = true });
        likeRes2.EnsureSuccessStatusCode();
        var likeResult2 = await likeRes2.Content.ReadFromJsonAsync<LikeResultDto>();
        Assert.True(likeResult2!.IsMatch);
        Assert.NotNull(likeResult2.MatchId);

        SetToken(_tokenA);
        var matchesA = await _client.GetFromJsonAsync<List<MatchDto>>("/api/matches");
        Assert.NotNull(matchesA);
        Assert.Single(matchesA);

        SetToken(_tokenB);
        var matchesB = await _client.GetFromJsonAsync<List<MatchDto>>("/api/matches");
        Assert.NotNull(matchesB);
        Assert.Single(matchesB);
        Assert.Equal(_userIdA, matchesB![0].OtherUserId);
    }

    [Fact]
    public async Task Discovery_ExcludesAlreadyLiked()
    {
        SetToken(_tokenA);
        await _client.PostAsJsonAsync("/api/discovery/like", new { toUserId = _userIdB, like = true });

        var disc = await _client.GetFromJsonAsync<List<DiscoveryProfileDto>>(
            "/api/discovery?activityType=motorcycle&maxDistanceKm=500");
        Assert.NotNull(disc);
        Assert.DoesNotContain(disc, p => p.Id == _userIdB);
    }

    [Fact]
    public async Task RemainingLikes_FreeUser_Returns50()
    {
        SetToken(_tokenA);
        var res = await _client.GetFromJsonAsync<RemainingLikesDto>("/api/discovery/likes/remaining");
        Assert.Equal(50, res!.Remaining);
    }

    private sealed class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    private sealed class LikeResultDto
    {
        public bool IsMatch { get; set; }
        public Guid? MatchId { get; set; }
    }

    private sealed class MatchDto
    {
        public Guid Id { get; set; }
        public Guid OtherUserId { get; set; }
    }

    private sealed class DiscoveryProfileDto
    {
        public Guid Id { get; set; }
    }

    private sealed class RemainingLikesDto
    {
        public int Remaining { get; set; }
    }
}
