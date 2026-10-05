-- EkubCircle Database Schema
-- 13 Tables
-- SQLite / Compatible with SQL Server, PostgreSQL

-- 1. Users
CREATE TABLE Users (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    PhoneNumber     NVARCHAR(20)   NOT NULL UNIQUE,
    Email           NVARCHAR(256)  NULL,
    FirstName       NVARCHAR(100)  NOT NULL,
    LastName        NVARCHAR(100)  NOT NULL,
    Gender          INTEGER         NOT NULL DEFAULT 0,  -- 0=Male, 1=Female, 2=Other
    JobType         NVARCHAR(100)  NOT NULL,
    Location        NVARCHAR(200)  NOT NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    IsPhoneVerified BIT             NOT NULL DEFAULT 0,
    IsPinEnabled    BIT             NOT NULL DEFAULT 0,
    PinHash         NVARCHAR(256)  NULL,
    PreferredLanguage INTEGER       NOT NULL DEFAULT 0, -- 0=English, 1=Amharic
    ReferralCode    NVARCHAR(20)   NOT NULL UNIQUE,
    ReferredById    INTEGER         NULL REFERENCES Users(Id) ON DELETE RESTRICT,
    IsAdmin         BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME        NOT NULL,
    UpdatedAt       DATETIME        NOT NULL
);

-- 2. PhoneVerifications
CREATE TABLE PhoneVerifications (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId           INTEGER NULL REFERENCES Users(Id) ON DELETE CASCADE,
    PhoneNumber      NVARCHAR(20)   NOT NULL,
    VerificationCode NVARCHAR(6)    NOT NULL,
    ExpiresAt       DATETIME        NOT NULL,
    IsUsed          BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME        NOT NULL
);

-- 3. Circles
CREATE TABLE Circles (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            NVARCHAR(200)  NOT NULL,
    Contribution    DECIMAL(18,2)  NOT NULL,
    MeetingLabel    NVARCHAR(50)   NOT NULL,
    Status          INTEGER         NOT NULL DEFAULT 0,  -- 0=Forming, 1=Active, 2=Completed
    OrganizerId     INTEGER         NOT NULL REFERENCES Users(Id) ON DELETE RESTRICT,
    CreatedAt       DATETIME        NOT NULL,
    StartedAt       DATETIME        NULL,
    CompletedAt     DATETIME        NULL
);

-- 4. CircleMembers
CREATE TABLE CircleMembers (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    CircleId        INTEGER NOT NULL REFERENCES Circles(Id) ON DELETE CASCADE,
    UserId          INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    PayoutOrder     INTEGER NOT NULL DEFAULT 0,
    HasReceived     BIT     NOT NULL DEFAULT 0,
    JoinedAt        DATETIME NOT NULL,
    UNIQUE (CircleId, UserId)
);

-- 5. Rounds
CREATE TABLE Rounds (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    CircleId        INTEGER NOT NULL REFERENCES Circles(Id) ON DELETE CASCADE,
    RoundNumber     INTEGER NOT NULL,
    Status          INTEGER NOT NULL DEFAULT 0,  -- 0=Pending, 1=Open, 2=PaidOut
    ReceiverId      INTEGER NULL REFERENCES Users(Id) ON DELETE RESTRICT,
    OpenedAt        DATETIME NOT NULL,
    PaidOutAt       DATETIME NULL,
    UNIQUE (CircleId, RoundNumber)
);

-- 6. Payments
CREATE TABLE Payments (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    RoundId         INTEGER NOT NULL REFERENCES Rounds(Id) ON DELETE CASCADE,
    UserId          INTEGER NOT NULL REFERENCES Users(Id) ON DELETE RESTRICT,
    HasPaid         BIT     NOT NULL DEFAULT 0,
    PaidAt          DATETIME NULL,
    LateFine        DECIMAL(18,2) NOT NULL DEFAULT 0,
    UNIQUE (RoundId, UserId)
);

-- 7. Notifications
CREATE TABLE Notifications (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId          INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Type            INTEGER NOT NULL DEFAULT 0,  -- 0=PaymentReminder, 1=Payout, 2=RoundOpened, 3=MemberJoined, 4=CircleStarted, 5=CircleCompleted, 6=General
    Title           NVARCHAR(200)  NOT NULL,
    Body            NVARCHAR(1000) NOT NULL,
    IsRead          BIT            NOT NULL DEFAULT 0,
    RelatedCircleId INTEGER       NULL,
    RelatedRoundId  INTEGER        NULL,
    CreatedAt       DATETIME       NOT NULL
);

-- 8. Feedbacks
CREATE TABLE Feedbacks (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId          INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Subject         NVARCHAR(200)  NOT NULL,
    Message         NVARCHAR(2000) NOT NULL,
    Status          INTEGER NOT NULL DEFAULT 0,  -- 0=Pending, 1=Reviewed
    CreatedAt       DATETIME NOT NULL
);

-- 9. SuccessStories
CREATE TABLE SuccessStories (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId          INTEGER NULL REFERENCES Users(Id) ON DELETE SET NULL,
    AuthorName      NVARCHAR(150)  NOT NULL,
    Content         NVARCHAR(2000) NOT NULL,
    Rating          INTEGER NOT NULL DEFAULT 5,  -- 1-5 stars
    IsApproved      BIT     NOT NULL DEFAULT 0,
    CreatedAt       DATETIME NOT NULL
);

-- 10. AccountDeletionRequests
CREATE TABLE AccountDeletionRequests (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId          INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    VerificationCode NVARCHAR(6)   NOT NULL,
    ExpiresAt       DATETIME NOT NULL,
    IsConfirmed     BIT     NOT NULL DEFAULT 0,
    CreatedAt       DATETIME NOT NULL
);

-- 11. EkubCategories
CREATE TABLE EkubCategories (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            NVARCHAR(100)  NOT NULL,
    Description     NVARCHAR(500)  NULL,
    IconUrl         NVARCHAR(500)  NULL,
    IsActive        BIT     NOT NULL DEFAULT 1,
    CreatedAt       DATETIME NOT NULL
);

-- 12. EkubSubCategories
CREATE TABLE EkubSubCategories (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryId      INTEGER NOT NULL REFERENCES EkubCategories(Id) ON DELETE CASCADE,
    Name            NVARCHAR(200)  NOT NULL,
    DailyContribution DECIMAL(18,2) NOT NULL,
    TotalRounds     INTEGER NOT NULL,
    TotalAmount     DECIMAL(18,2) NOT NULL,
    StartDate       DATETIME NOT NULL,
    TermsAndConditions TEXT NOT NULL,
    MaxMembers      INTEGER NOT NULL,
    CurrentMemberCount INTEGER NOT NULL DEFAULT 0,
    Status          INTEGER NOT NULL DEFAULT 0,  -- 0=Open, 1=Full, 2=Started, 3=Completed
    CreatedByAdminId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE RESTRICT,
    CircleId        INTEGER NULL REFERENCES Circles(Id) ON DELETE RESTRICT,
    CreatedAt       DATETIME NOT NULL
);

-- 13. EkubSubscriptions
CREATE TABLE EkubSubscriptions (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId          INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    SubCategoryId   INTEGER NOT NULL REFERENCES EkubSubCategories(Id) ON DELETE CASCADE,
    AgreedToTerms   BIT     NOT NULL DEFAULT 0,
    JoinedAt        DATETIME NOT NULL,
    UNIQUE (UserId, SubCategoryId)
);
