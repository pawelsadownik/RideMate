using RideMate.Domain.Common;
using System.Net.Mail;

namespace RideMate.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpires { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpires { get; private set; }
    public DateTime LastActiveAt { get; private set; } = DateTime.UtcNow;

    // Navigacja
    public Profile? Profile { get; private set; }
    public ICollection<Like> SentLikes { get; private set; } = new List<Like>();
    public ICollection<Like> ReceivedLikes { get; private set; } = new List<Like>();
    public ICollection<Match> Matches { get; private set; } = new List<Match>();
    public ICollection<Conversation> Conversations { get; private set; } = new List<Conversation>();
    public ICollection<Message> Messages { get; private set; } = new List<Message>();
    public ICollection<Block> BlocksAsBlocker { get; private set; } = new List<Block>();
    public ICollection<Block> BlocksAsBlocked { get; private set; } = new List<Block>();
    public ICollection<Report> ReportsAsReporter { get; private set; } = new List<Report>();
    public ICollection<Report> ReportsAsReported { get; private set; } = new List<Report>();
    public SubscriptionEntitlement? Subscription { get; private set; }
    public ICollection<PushToken> PushTokens { get; private set; } = new List<PushToken>();

    private User() { }

    public static User Create(string email, string passwordHash, DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail nie może być pusty.", nameof(email));
        if (!IsValidEmail(email))
            throw new ArgumentException("Nieprawidłowy format adresu e-mail.", nameof(email));
        if (dateOfBirth > DateTime.UtcNow.AddYears(-18))
            throw new ArgumentException("Użytkownik musi być pełnoletni (18+).", nameof(dateOfBirth));

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            DateOfBirth = dateOfBirth,
            IsEmailConfirmed = false,
            IsDeleted = false
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email.Trim().ToLowerInvariant();
        }
        catch
        {
            return false;
        }
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpires = null;
        Touch();
    }

    public void SetEmailConfirmationToken(string token, DateTime expiresAt)
    {
        EmailConfirmationToken = token;
        EmailConfirmationTokenExpires = expiresAt;
        Touch();
    }

    public void SetPasswordResetToken(string token, DateTime expiresAt)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpires = expiresAt;
        Touch();
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpires = null;
        Touch();
    }

    public void UpdateLastActive() => LastActiveAt = DateTime.UtcNow;
}
