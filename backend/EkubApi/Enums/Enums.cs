namespace EkubApi.Enums;

/// <summary>
/// Lifecycle of a circle: members join while Forming, the circle runs while Active,
/// and completes when every member has received the pot once.
/// </summary>
public enum CircleStatus
{
    Forming = 0,
    Active = 1,
    Completed = 2
}

/// <summary>
/// Lifecycle of a round: Pending means it exists but hasn't been opened yet,
/// Open means it's accepting payments, PaidOut means the pot has been awarded.
/// </summary>
public enum RoundStatus
{
    Pending = 0,
    Open = 1,
    PaidOut = 2
}

/// <summary>
/// User gender for profile data.
/// </summary>
public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2
}

/// <summary>
/// Preferred app language. Amharic and English supported.
/// </summary>
public enum Language
{
    English = 0,
    Amharic = 1
}

/// <summary>
/// Types of notifications shown in the notification bell.
/// </summary>
public enum NotificationType
{
    PaymentReminder = 0,
    PayoutNotification = 1,
    RoundOpened = 2,
    MemberJoined = 3,
    CircleStarted = 4,
    CircleCompleted = 5,
    General = 6
}

/// <summary>
/// Status of user-submitted feedback/questions.
/// </summary>
public enum FeedbackStatus
{
    Pending = 0,
    Reviewed = 1
}

/// <summary>
/// Status of a public Ekub sub-category in the catalog.
/// </summary>
public enum EkubSubCategoryStatus
{
    Open = 0,       // Users can join
    Full = 1,       // Max members reached
    Started = 2,    // Circle created and running
    Completed = 3   // Circle finished
}
