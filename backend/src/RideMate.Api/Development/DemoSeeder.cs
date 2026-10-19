using Microsoft.EntityFrameworkCore;
using RideMate.Application.Interfaces;
using RideMate.Domain;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Api.Development;

public static class DemoSeeder
{
    public const string Password = "RideMate123!";
    public static async Task SeedAsync(RideMateDbContext db, IPasswordHasher hasher, IConfiguration config)
    {
        if (await db.Users.AnyAsync()) return;
        var names = new[] { "Marek", "Kasia", "Tomek", "Anna", "Piotr", "Ola", "Michał", "Magda", "Paweł", "Natalia", "Kamil", "Ewa", "Adam", "Julia", "Bartek", "Zosia", "Wojtek", "Karolina", "Łukasz", "Monika" };
        var ages = new[] { 24, 26, 30, 28, 28, 27, 35, 30, 33, 25, 26, 32, 32, 29, 27, 26, 38, 34, 29, 28 };
        var cities = new[] { "Gdańsk", "Gdynia", "Sopot", "Pruszcz Gdański", "Żukowo" };
        var styles = new[] { "Touring", "Rekreacyjny", "Gravel", "Adventure" };
        var bios = new[] { "Weekendowe wypady, górskie trasy i dobra kawa po drodze. Profil testowy.", "Szukam ekipy na spokojne przejażdżki nad morzem. Profil testowy.", "Lubię długie trasy, nowe miejsca i rozmowy przy kawie. Profil testowy.", "Najlepszy plan na weekend? Wspólna jazda i zachód słońca. Profil testowy." };
        var baseUrl = (config["Demo:PublicApiUrl"] ?? "http://localhost:5050").TrimEnd('/');
        var hash = hasher.Hash(Password);
        var users = new List<User>();
        for (var i = 0; i < 20; i++)
        {
            var age = ages[i];
            var user = User.Create($"demo{i + 1:00}@ridemate.test", hash, DateTime.UtcNow.Date.AddYears(-age).AddDays(-10));
            user.ConfirmEmail();
            users.Add(user);
            db.Users.Add(user);
            var activity = (ActivityType)(i % 3 + 1);
            var profile = Profile.Create(user.Id, names[i], age, (LookingFor)(i % 3 + 1), activity, styles[i % styles.Length], bios[i % bios.Length], cities[i % cities.Length], 54.352 + (i % 5) * .018, 18.646 + (i / 5) * .02);
            db.Profiles.Add(profile);
            db.ProfilePhotos.Add(ProfilePhoto.Create(profile.Id, $"{baseUrl}/demo/{i + 1:00}.jpg", $"demo/{i + 1:00}.jpg", true, 0));
            var preferences = RidingPreferences.Create(profile.Id);
            preferences.UpdateMotorcycle("Touring", "Honda", "CB500X", activity != ActivityType.Bicycle, i % 2 == 0, false);
            preferences.UpdateBicycles(i % 2 == 0, true, i % 3 == 0, false, true, true);
            db.RidingPreferences.Add(preferences);
            db.SubscriptionEntitlements.Add(SubscriptionEntitlement.Create(user.Id));
        }
        // Ready-to-trigger mutual likes when demo01 likes demo02..demo06.
        foreach (var user in users.Skip(1).Take(5)) db.Likes.Add(Like.Create(user.Id, users[0].Id, LikeType.Like));
        // One existing conversation for testing chat immediately after login.
        var match = Match.Create(users[0].Id, users[19].Id);
        db.Likes.Add(Like.Create(users[0].Id, users[19].Id, LikeType.Like));
        db.Likes.Add(Like.Create(users[19].Id, users[0].Id, LikeType.Like));
        db.Matches.Add(match);
        var conversation = Conversation.Create(match.Id);
        db.Conversations.Add(conversation);
        db.Messages.Add(Message.Create(conversation.Id, users[19].Id, "Cześć! Masz ochotę na weekendową przejażdżkę? 🏍️"));
        db.Messages.Add(Message.Create(conversation.Id, users[0].Id, "Jasne! Może nad morze i kawa po drodze?"));
        await db.SaveChangesAsync();
    }
}
