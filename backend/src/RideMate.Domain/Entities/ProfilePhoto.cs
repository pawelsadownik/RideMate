using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class ProfilePhoto : BaseEntity
{
    public Guid ProfileId { get; private set; }
    public string Url { get; private set; } = string.Empty;        // publiczny URL (CDN/storage)
    public string StoragePath { get; private set; } = string.Empty; // ścieżka w storage (do usunięcia)
    public bool IsPrimary { get; private set; }
    public int OrderIndex { get; private set; }

    public Profile? Profile { get; private set; }

    private ProfilePhoto() { }

    public static ProfilePhoto Create(Guid profileId, string url, string storagePath, bool isPrimary, int orderIndex)
    {
        return new ProfilePhoto
        {
            ProfileId = profileId,
            Url = url,
            StoragePath = storagePath,
            IsPrimary = isPrimary,
            OrderIndex = orderIndex
        };
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
        Touch();
    }

    public void Reorder(int orderIndex)
    {
        OrderIndex = orderIndex;
        Touch();
    }
}
