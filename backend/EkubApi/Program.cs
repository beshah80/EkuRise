using System.Security.Cryptography;
using System.Text;
using EkubApi.Data;
using EkubApi.Entities;
using EkubApi.Enums;
using EkubApi.Middleware;
using EkubApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var activeConnection = ResolveSupabaseConnection(defaultConnection);

// --- Database ---
builder.Services.AddDbContext<EkubDbContext>(options =>
    options.UseNpgsql(activeConnection, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// --- Services (DI) ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICircleService, CircleService>();
builder.Services.AddScoped<IRoundService, RoundService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// --- JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? "EkubCircleSecretKey2026AtLeast32Characters!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EkubApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EkubClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- CORS for Angular frontend ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- Controllers ---
builder.Services.AddControllers();

var app = builder.Build();

// Railway sets PORT env var — bind to it
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// --- Exception handling middleware ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

// --- Migrate and seed database ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EkubDbContext>();
    var databaseCreator = db.GetService<IRelationalDatabaseCreator>();
    
    var conn = db.Database.GetDbConnection();
    await conn.OpenAsync();
    bool usersTableExists;
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND lower(table_name) = 'users');";
        usersTableExists = Convert.ToBoolean(await cmd.ExecuteScalarAsync());
    }

    if (!usersTableExists)
    {
        Console.WriteLine("[DB] Application tables not found in Supabase public schema. Creating tables...");
        await databaseCreator.CreateTablesAsync();
        Console.WriteLine("[DB] Supabase tables created successfully.");
    }

    Console.WriteLine("[DB] Checking and seeding default data...");
    await SeedDataAsync(db);
    Console.WriteLine("[DB] Database ready.");
}

// --- HTTP pipeline ---
app.UseCors("AngularClient");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // Swagger/OpenAPI can be added with Microsoft.AspNetCore.OpenApi package
}

app.MapControllers();

// Simple health check so the browser shows something at the root
app.MapGet("/", () => new { status = "running", app = "EkubCircle API", version = "1.0" });
app.MapGet("/health", () => new { status = "healthy" });

app.Run();

// --- Seed demo users with phone-based profiles ---
static async Task SeedDataAsync(EkubDbContext db)
{
    if (!await db.Users.AnyAsync())
    {
        var users = new[]
        {
            new User
            {
                PhoneNumber = "0912345678",
                FirstName = "Admin",
                LastName = "Organizer",
                Gender = Gender.Male,
                JobType = "Business Owner",
                Location = "Addis Ababa",
                IsPhoneVerified = true,
                IsPinEnabled = true,
                PinHash = HashPin("1234"),
                ReferralCode = "EK-ADOR-1001",
                PreferredLanguage = Language.English,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                PhoneNumber = "0987654321",
                FirstName = "Standard",
                LastName = "Member",
                Gender = Gender.Female,
                JobType = "Teacher",
                Location = "Bahir Dar",
                IsPhoneVerified = true,
                IsPinEnabled = true,
                PinHash = HashPin("1234"),
                ReferralCode = "EK-STME-1002",
                PreferredLanguage = Language.English,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                PhoneNumber = "0911112222",
                FirstName = "Abeba",
                LastName = "Tadesse",
                Gender = Gender.Female,
                JobType = "Merchant",
                Location = "Addis Ababa",
                IsPhoneVerified = true,
                IsPinEnabled = true,
                PinHash = HashPin("1234"),
                ReferralCode = "EK-ABTA-1003",
                PreferredLanguage = Language.Amharic,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                PhoneNumber = "0922223333",
                FirstName = "Kidane",
                LastName = "Bekele",
                Gender = Gender.Male,
                JobType = "Driver",
                Location = "Hawassa",
                IsPhoneVerified = true,
                IsPinEnabled = true,
                PinHash = HashPin("1234"),
                ReferralCode = "EK-KIBE-1004",
                PreferredLanguage = Language.Amharic,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                PhoneNumber = "0933334444",
                FirstName = "Dawit",
                LastName = "Haile",
                Gender = Gender.Male,
                JobType = "Engineer",
                Location = "Addis Ababa",
                IsPhoneVerified = true,
                IsPinEnabled = true,
                PinHash = HashPin("1234"),
                ReferralCode = "EK-DAHA-1005",
                PreferredLanguage = Language.English,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                PhoneNumber = "0944445555",
                FirstName = "Sara",
                LastName = "Girma",
                Gender = Gender.Female,
                JobType = "Accountant",
                Location = "Gondar",
                IsPhoneVerified = true,
                IsPinEnabled = true,
                PinHash = HashPin("1234"),
                ReferralCode = "EK-SAGI-1006",
                PreferredLanguage = Language.Amharic,
                CreatedAt = DateTime.UtcNow
            }
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();
    }

    var categories = await db.EkubCategories.ToListAsync();
    if (!categories.Any())
    {
        categories = new List<EkubCategory>
        {
            new EkubCategory { Name = "Driver Equb", Description = "Ekub for drivers — daily contributions, fast payouts.", IconUrl = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new EkubCategory { Name = "Trader Equb", Description = "Ekub for merchants and traders to grow their business.", IconUrl = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new EkubCategory { Name = "Workers Equb", Description = "Ekub for office workers and salaried employees.", IconUrl = null, IsActive = true, CreatedAt = DateTime.UtcNow },
            new EkubCategory { Name = "Ye Ayinet Equb", Description = "Mixed group Ekub — open to all professions.", IconUrl = null, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        db.EkubCategories.AddRange(categories);
        await db.SaveChangesAsync();
    }

    if (!await db.EkubSubCategories.AnyAsync())
    {
        var subCategories = new[]
        {
            new EkubSubCategory
            {
                CategoryId = categories[0].Id, Name = "Daily 300 ETB", DailyContribution = 300, TotalRounds = 105,
                TotalAmount = 31500, StartDate = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc),
                TermsAndConditions = "By joining this Driver Equb, you agree to pay 300 ETB daily for 105 days. The winner of each round is drawn randomly and fairly. Late payments may incur a 50 ETB fine per day. Missing 3 consecutive days results in removal from the circle. The organizer manages all rounds and payouts.",
                MaxMembers = 105, CurrentMemberCount = 1, Status = EkubSubCategoryStatus.Open,
                CreatedByAdminId = 1, CreatedAt = DateTime.UtcNow
            },
            new EkubSubCategory
            {
                CategoryId = categories[0].Id, Name = "Daily 500 ETB", DailyContribution = 500, TotalRounds = 60,
                TotalAmount = 30000, StartDate = new DateTime(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc),
                TermsAndConditions = "By joining this Driver Equb, you agree to pay 500 ETB daily for 60 days. Each round winner is drawn randomly and fairly. Late fine is 50 ETB per day. Missing 2 consecutive days results in removal. The organizer manages all rounds and payouts.",
                MaxMembers = 60, CurrentMemberCount = 1, Status = EkubSubCategoryStatus.Open,
                CreatedByAdminId = 1, CreatedAt = DateTime.UtcNow
            },
            new EkubSubCategory
            {
                CategoryId = categories[1].Id, Name = "Daily 1000 ETB", DailyContribution = 1000, TotalRounds = 50,
                TotalAmount = 50000, StartDate = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                TermsAndConditions = "By joining this Trader Equb, you agree to pay 1000 ETB daily for 50 days. The total pot is 50,000 ETB per round. Each round winner is drawn randomly and fairly. Late fine is 100 ETB per day. Missing 2 consecutive days results in removal.",
                MaxMembers = 50, CurrentMemberCount = 1, Status = EkubSubCategoryStatus.Open,
                CreatedByAdminId = 1, CreatedAt = DateTime.UtcNow
            },
            new EkubSubCategory
            {
                CategoryId = categories[2].Id, Name = "Monthly 2000 ETB", DailyContribution = 2000, TotalRounds = 12,
                TotalAmount = 24000, StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                TermsAndConditions = "By joining this Workers Equb, you agree to pay 2000 ETB monthly for 12 months. Each round winner is drawn randomly and fairly. Late fine is 200 ETB per week. The organizer manages all rounds and payouts.",
                MaxMembers = 12, CurrentMemberCount = 1, Status = EkubSubCategoryStatus.Open,
                CreatedByAdminId = 1, CreatedAt = DateTime.UtcNow
            },
            new EkubSubCategory
            {
                CategoryId = categories[3].Id, Name = "Daily 200 ETB", DailyContribution = 200, TotalRounds = 150,
                TotalAmount = 30000, StartDate = new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc),
                TermsAndConditions = "By joining this Ye Ayinet Equb, you agree to pay 200 ETB daily for 150 days. Open to all professions. Each round winner is drawn randomly and fairly. Late fine is 50 ETB per day. The organizer manages all rounds and payouts.",
                MaxMembers = 150, CurrentMemberCount = 1, Status = EkubSubCategoryStatus.Open,
                CreatedByAdminId = 1, CreatedAt = DateTime.UtcNow
            }
        };
        db.EkubSubCategories.AddRange(subCategories);
        await db.SaveChangesAsync();

        // Auto-subscribe admin to each sub-category (admin is organizer + member)
        foreach (var sub in subCategories)
        {
            db.EkubSubscriptions.Add(new EkubSubscription
            {
                UserId = 1,
                SubCategoryId = sub.Id,
                AgreedToTerms = true,
                Status = SubscriptionStatus.Approved,
                ApprovedAt = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();
    }
}

static string HashPin(string pin)
{
    var salt = RandomNumberGenerator.GetBytes(16);
    var hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, 100_000, HashAlgorithmName.SHA256, 32);
    return $"{Convert.ToBase64String(hash)}.{Convert.ToBase64String(salt)}.{100_000}";
}

static string ResolveSupabaseConnection(string configuredConnStr)
{
    try
    {
        using var testConn = new Npgsql.NpgsqlConnection(configuredConnStr);
        testConn.Open();
        Console.WriteLine("[DB CONNECT] Successfully connected using configured connection string.");
        return configuredConnStr;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB CONNECT] Configured connection string test failed: {ex.Message}");
    }

    var regions = new[]
    {
        "eu-west-1", "eu-west-2", "eu-west-3", "eu-central-1", "eu-central-2", "eu-north-1",
        "us-east-1", "us-east-2", "us-west-1", "us-west-2",
        "ca-central-1", "sa-east-1", "me-central-1", "af-south-1",
        "ap-south-1", "ap-southeast-1", "ap-northeast-1", "ap-northeast-2", "ap-southeast-2"
    };

    foreach (var r in regions)
    {
        foreach (var port in new[] { 5432, 6543 })
        {
            var candidate = $"Host=aws-0-{r}.pooler.supabase.com;Port={port};Database=postgres;Username=postgres.faaumaigqygmdwtxvkzp;Password=pCPz&5ug8!d7qEm;SSL Mode=Require;Trust Server Certificate=true;Timeout=5";
            try
            {
                using var conn = new Npgsql.NpgsqlConnection(candidate);
                conn.Open();
                Console.WriteLine($"[DB CONNECT] Successfully connected to Supabase pooler: {r} (Port {port})!");
                return candidate;
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                // Next region
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB CONNECT] Tried {r}:{port} -> {ex.Message}");
            }
        }
    }

    return configuredConnStr;
}

