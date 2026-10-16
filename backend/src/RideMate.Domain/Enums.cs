namespace RideMate.Domain;

public enum ActivityType
{
    Motorcycle = 1,
    Bicycle = 2,
    Both = 3
}

public enum LookingFor
{
    Friendship = 1,
    JointRides = 2,
    PotentialRelationship = 3
}

public enum LikeType
{
    Like = 1,
    Pass = 2
}

public enum ReportStatus
{
    Open = 1,
    UnderReview = 2,
    Resolved = 3,
    Dismissed = 4
}

public enum SubscriptionPlatform
{
    None = 0,
    Apple = 1,
    Google = 2,
    Development = 99
}
