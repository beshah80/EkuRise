namespace TmsApi.Domain.Enums;

public enum CircleStatus { Forming = 0, Active = 1, Completed = 2 }
public enum RoundStatus { Pending = 0, Open = 1, PaidOut = 2 }
public enum Gender { Male = 0, Female = 1, Other = 2 }
public enum Language { English = 0, Amharic = 1 }
public enum NotificationType { PaymentReminder = 0, PayoutNotification = 1, RoundOpened = 2, MemberJoined = 3, CircleStarted = 4, CircleCompleted = 5, General = 6 }
public enum FeedbackStatus { Pending = 0, Reviewed = 1 }
public enum EkubSubCategoryStatus { Open = 0, Full = 1, Started = 2, Completed = 3 }
