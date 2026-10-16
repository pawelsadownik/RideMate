using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class Profile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Nickname { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public string? City { get; private set; }
    public double? Latitude { get; private set; }   // przybliżona lokalizacja (miasto)
    public double? Longitude { get; private set; }
    public LookingFor LookingFor { get; private set; } = LookingFor.JointRides;
    public ActivityType ActivityType { get; private set; } = ActivityType.Both;
    public string? RidingStyle { get; private set; }
    public int Age { get; private set; }
    public bool IsActive { get; private set; } = true;

    public User? User { get; private set; }
    public ICollection<ProfilePhoto> Photos { get; private set; } = new List<ProfilePhoto>();
    public RidingPreferences? RidingPreferences { get; set; }

    private Profile() { }

    public static Profile Create(
        Guid userId,
        string nickname,
        int age,
        LookingFor lookingFor,
        ActivityType activityType,
        string? ridingStyle,
        string? bio,
        string? city,
        double? latitude,
        double? longitude)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("Pseudonim nie może być pusty.", nameof(nickname));
        if (nickname.Trim().Length < 2 || nickname.Trim().Length > 30)
            throw new ArgumentException("Pseudonim musi mieć od 2 do 30 znaków.", nameof(nickname));

        return new Profile
        {
            UserId = userId,
            Nickname = nickname.Trim(),
            Age = age,
            LookingFor = lookingFor,
            ActivityType = activityType,
            RidingStyle = ridingStyle,
            Bio = bio,
            City = city,
            Latitude = latitude,
            Longitude = longitude,
            IsActive = true
        };
    }

    public void Update(
        string nickname,
        int age,
        LookingFor lookingFor,
        ActivityType activityType,
        string? ridingStyle,
        string? bio,
        string? city,
        double? latitude,
        double? longitude)
    {
        Nickname = nickname.Trim();
        Age = age;
        LookingFor = lookingFor;
        ActivityType = activityType;
        RidingStyle = ridingStyle;
        Bio = bio;
        City = city;
        Latitude = latitude;
        Longitude = longitude;
        Touch();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
}
