# Ekub API Investigation Report

## Summary

The backend is **substantially implemented and mostly correct**. The core Ekub rules (fixed payout order, receive-once guard, all-paid gate, state machine) are in the code and enforced. However, there are **6 real bugs or gaps** that would cause failures at runtime, plus a **critical infrastructure mismatch** that prevents the app from starting at all. The biggest single problem is that the `.csproj` declares `Microsoft.EntityFrameworkCore.Sqlite` but `Program.cs` calls `UseNpgsql()` — the app cannot start on any machine without a Supabase Postgres connection, and the Supabase password is hardcoded in `appsettings.json` (a security problem for a public repo). Separately, the "Member home" screen (paid this round / received / pot so far / winner history) has no dedicated API endpoint; the member must assemble that from multiple calls.

---

## 1. What Works

### 1.1 State Machine — Circle
**File:** `Enums/Enums.cs`, `Entities/Circle.cs`, `Services/CircleService.cs`

- `CircleStatus` enum defines `Forming → Active → Completed` correctly.
- `CreateCircleAsync` initialises to `Forming`.
- `StartCircleAsync` transitions to `Active`, sets `StartedAt`. Guard `EnsureForming()` blocks start if not Forming.
- `PayOutAsync` (RoundService) transitions to `Completed` when `AllAsync(cm => cm.HasReceived)` — every member has received.
- `CompletedAt` is stamped.

### 1.2 State Machine — Round
**File:** `Enums/Enums.cs` (`Pending = 0, Open = 1, PaidOut = 2`), `Services/CircleService.cs`, `Services/RoundService.cs`

- Start creates rounds: round 1 is `Open`, rounds 2…N are `Pending`. ✓
- `OpenNextRoundAsync` finds the lowest-numbered `Pending` round, marks it `Open`, stamps `OpenedAt`, and creates payment rows. ✓
- `PayOutAsync` marks the round `PaidOut` and stamps `PaidOutAt`. ✓

### 1.3 Three Hard Rules (Payout)
**File:** `Services/RoundService.cs` — `PayOutAsync`

1. **Block payout on an already-paid-out round** — checks `round.Status == RoundStatus.PaidOut` first. ✓
2. **Block payout before everyone has paid** — counts `!p.HasPaid` rows; throws with names if any exist. ✓
3. **Block second payout to same member** — checks `receiver.HasReceived`; throws. ✓

### 1.4 Fixed Payout Order (Not Typed In)
**File:** `Services/CircleService.cs` — `StartCircleAsync`, `Services/RoundService.cs` — `PayOutAsync`

`PayoutOrder` is assigned at start (`i + 1` based on join order). The receiver lookup is:
```csharp
var receiver = await _db.CircleMembers
    .FirstOrDefaultAsync(cm => cm.CircleId == circleId && cm.PayoutOrder == round.RoundNumber);
```
No name is typed into a box. ✓

### 1.5 Authentication
**File:** `Services/AuthService.cs`, `Controllers/AuthController.cs`

- Two-step phone registration with OTP. ✓
- PIN login with PBKDF2-SHA256 hash (`Rfc2898DeriveBytes.Pbkdf2`, 100k iterations, random 16-byte salt). ✓
- OTP login. ✓
- `BaseController.GetUserId()` reads `ClaimTypes.NameIdentifier` from the JWT. ✓
- JWT validated with issuer/audience/signing-key/lifetime; `ClockSkew = Zero`. ✓

### 1.6 Create Circle
**File:** `Services/CircleService.cs` — `CreateCircleAsync`  
**DTO:** `DTOs/CircleDtos.cs` — `CreateCircleDto`

- `Name`, `Contribution`, `MeetingLabel` accepted and validated. ✓
- Organizer auto-added as first member. ✓

### 1.7 Add/Remove Members While Forming
- `AddMemberAsync` looks up by phone number, enforces `EnsureForming`, prevents duplicates. ✓
- `RemoveMemberAsync` enforces `EnsureForming`, prevents removing the organizer. ✓

### 1.8 Start Circle
- Locks member list (payout order assigned, immutable after). ✓
- Creates N rounds (one per member). ✓
- First round opened immediately. ✓
- Payment rows created for first round. ✓
- Minimum 2 members enforced. ✓

### 1.9 Mark Payment
**File:** `Services/RoundService.cs` — `MarkPaymentAsync`

- Organizer-only (EnsureOrganizer). ✓
- Only on Open rounds. ✓
- Supports toggling `HasPaid` and optional `LateFine`. ✓

### 1.10 Round History + Filtering
**File:** `Controllers/RoundsController.cs`, `Services/RoundService.cs` — `GetRoundsAsync`

- GET `api/circles/{circleId}/rounds` supports `?roundNumber=` and `?status=` query params. ✓
- Returns `RoundSummaryDto` list ordered by `RoundNumber`. ✓

### 1.11 Round Detail (Paid/Unpaid Screen)
- `RoundDetailDto` includes: pot (`PaidCount × Contribution`), each member's `HasPaid`, `ReceiverId`, `ReceiverName`. ✓
- `GET .../rounds/current` fetches the open round. ✓

### 1.12 Database Schema
- `CircleMember` has unique index on `(CircleId, UserId)` — prevents duplicate membership. ✓
- `Payment` has unique index on `(RoundId, UserId)` — prevents duplicate payment rows. ✓
- `Round` has unique index on `(CircleId, RoundNumber)`. ✓
- Foreign key cascade rules are consistent. ✓

---

## 2. Bugs and Broken Items

### BUG-1 (CRITICAL): Database provider mismatch — app cannot start
**File:** `EkubApi.csproj` line 8, `Program.cs` line 16

`.csproj` includes:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.12" />
```
But `Program.cs` registers:
```csharp
builder.Services.AddDbContext<EkubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```
`UseNpgsql` is an extension method from the `Npgsql.EntityFrameworkCore.PostgreSQL` package, which is **not listed** in the `.csproj`. The build will fail with a `CS0103: The name 'UseNpgsql' does not exist` error (or a missing assembly error), making the app unrunnable locally.

There is also an `ekub.db` SQLite file in the project root, which suggests the project was previously using SQLite and was partially migrated to Postgres without adding the Npgsql package or removing the SQLite one.

**Fix:** Either:
- Add `Npgsql.EntityFrameworkCore.PostgreSQL` to the `.csproj` (to keep Postgres with Supabase), or
- Switch back to `UseSqlite("Data Source=ekub.db")` with the existing SQLite package (simpler for local dev/hackathon).

### BUG-2 (SECURITY): Database credentials hardcoded in `appsettings.json`
**File:** `appsettings.json`

The Supabase connection string (host, port, username, password) is committed in plain text. If this repo is public, credentials are exposed.

**Fix:** Move secrets to `appsettings.Development.json` (excluded from git) or environment variables.

### BUG-3 (LOGIC): `OpenNextRoundAsync` ignores the `roundId` route parameter
**File:** `Controllers/RoundsController.cs` — `OpenNext` action, `Services/RoundService.cs` — `OpenNextRoundAsync`

The controller route is `POST api/circles/{circleId}/rounds/{roundId}/next`, but the service signature is:
```csharp
Task<RoundDetailDto> OpenNextRoundAsync(int circleId, int organizerId);
```
`roundId` is received by the controller but never passed to the service. The service always finds the lowest-numbered `Pending` round regardless. This is actually fine functionally (it can't go wrong), but the API surface is misleading: callers think they're targeting a specific round, which they're not.

**Fix:** Either remove `{roundId}` from the route and make it `POST api/circles/{circleId}/rounds/next`, or validate that the currently-paid-out round is indeed the one identified by `roundId` before opening the next.

### BUG-4 (LOGIC): Payment rows created twice for the first round
**Files:** `Services/CircleService.cs` — `StartCircleAsync`, `Services/RoundService.cs` — `CreatePaymentRowsForRoundAsync`

`StartCircleAsync` calls `CreatePaymentRowsForRoundAsync(rounds[0].Id, circleId)` to seed the first round's payments. `OpenNextRoundAsync` also calls `CreatePaymentRowsForRoundAsync` when opening a pending round. Since round 1 is opened at Start (not via `OpenNextRound`), this is correct — round 1 gets one payment-row creation call.

However, the `Payment` table has a unique index on `(RoundId, UserId)`. If someone were to call `OpenNextRoundAsync` when round 1 is still open (blocked by the "already open" guard) this is fine. But there is a subtle duplication risk: `CircleService` and `RoundService` both define `CreatePaymentRowsForRoundAsync` as private/internal methods with identical logic. If a future refactor calls both, it will hit a DB unique-constraint violation.

**Fix:** Extract `CreatePaymentRowsForRoundAsync` into a shared service or the DbContext, and use a single canonical call.

### BUG-5 (LOGIC): `GetRoundsAsync` authorization — any authenticated user can list any circle's rounds
**File:** `Services/RoundService.cs` — `GetRoundsAsync`, `Controllers/RoundsController.cs` — `GetRounds`

`GetCurrentRoundAsync` and `GetRoundByIdAsync` both gate on circle membership:
```csharp
var isMember = await _db.CircleMembers.AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId);
if (!isMember) return null;
```
But `GetRoundsAsync` has **no membership check at all**. Any logged-in user can call `GET api/circles/{circleId}/rounds` and enumerate all rounds (and their payment/receiver data) for any circle, even one they don't belong to.

**Fix:** Add the membership check to `GetRoundsAsync`.

### BUG-6 (AUTH): No "organizer only" enforcement at the HTTP layer — relies purely on service exception
**Files:** `Controllers/CirclesController.cs`, `Controllers/RoundsController.cs`

`EnsureOrganizer` in both services throws `UnauthorizedAccessException`, which `ExceptionHandlingMiddleware` maps to HTTP 403. This works — but the exception mapping is:
```csharp
UnauthorizedAccessException => (HttpStatusCode.Forbidden, ex.Message),
```
HTTP 403 Forbidden is correct for "you're logged in but not allowed." However, `GetUserId()` throws `UnauthorizedAccessException` when the JWT claim is missing, and that too maps to 403 (not 401 Unauthorized). A client that receives 403 on a missing-token request will be confused — it should be 401.

**Fix:** In `BaseController.GetUserId()`, use `HttpContext.Response.StatusCode = 401` before throwing, or throw a custom exception type that the middleware maps to 401.

---

## 3. Missing Features (Not Implemented)

### MISSING-1: Member Home endpoint — no dedicated summary view
**Required by spec:** "Member home: paid this round or not, received or not, pot so far, history of winners."

There is no single endpoint that returns:
- Whether the calling member has paid the current round
- Whether they have received their pot
- Pot accumulated so far (this round)
- History of past round winners

A member must currently make at least 3 calls:
1. `GET api/circles/{id}` — for basic circle info and their own `HasReceived`
2. `GET api/circles/{id}/rounds/current` — for their payment row and pot
3. `GET api/circles/{id}/rounds?status=PaidOut` — for winner history

**Recommended fix:** Add `GET api/circles/{circleId}/my-status` that returns a `MemberHomeDto` bundling all of this. This is a pure read that the existing service layer can answer without new DB queries.

### MISSING-2: No `[Authorize]` on `RoundsController` actions individually
**File:** `Controllers/RoundsController.cs`

`RoundsController` inherits `BaseController` which has `[Authorize]`. That propagates to all actions. This is fine — all routes are protected. Just noting it is correct by inheritance, not explicit attribute, which could be missed in a future refactor if the base class changes.

### MISSING-3: No notification dispatch on key events
**File:** `Services/NotificationService.cs`

The `NotificationsController` and `INotificationService` exist, but neither `CircleService` nor `RoundService` call `INotificationService` to push notifications (e.g., `CircleStarted`, `PayoutNotification`, `PaymentReminder`). The notification system is a dead-letter box — records can be read but nothing writes to it from business logic.

**Impact:** Low for MVP (the spec doesn't mandate push notifications), but the front-end notification bell will always be empty.

### MISSING-4: Round completion → auto-opening the next round (UX gap, not hard-broken)
**Required by spec:** "Organizer pays out the current round, then opens the next."

The payout endpoint (`POST .../payout`) does not automatically open the next round. The organizer must make a second call to `POST .../next`. This is a two-step flow, which is correct per the spec. But there is no guard that prevents opening a round that hasn't been paid out yet (just that an open round already exists). The `OpenNextRoundAsync` checks:
```csharp
var openRound = await _db.Rounds.Where(r => r.Status == RoundStatus.Open).FirstOrDefaultAsync();
if (openRound is not null) throw ...;
```
This correctly prevents opening a second concurrent round. ✓

### MISSING-5: `GET /api/circles/{circleId}/rounds` — no member guard (same as BUG-5 above)
Restated here as a missing feature: the authorization gate on the rounds list view was not implemented.

---

## 4. Minor Observations

| # | Item | File | Severity |
|---|------|------|----------|
| M1 | `OtpSentDto` returns the OTP code in the response body (`DemoCode`). Fine for a hackathon; must not reach production. | `DTOs/AuthDtos.cs`, `Services/AuthService.cs` | Info |
| M2 | `Pot` in `RoundDetailDto` is `PaidCount × Contribution`, not `TotalMembers × Contribution`. This is correct — it reflects actual cash collected, not the promise. | `Services/RoundService.cs` | Info |
| M3 | `CircleDetailDto` does not carry `HasReceived` for the *calling* member specifically — the client must search the `Members` list for themselves. | `DTOs/CircleDtos.cs` | Info |
| M4 | The Catalog/Admin/Feedback/SuccessStory/Subscription modules are implemented but out of scope for the Ekub MVP ledger. They don't break anything. | `Controllers/CatalogController.cs`, `Controllers/AdminController.cs` etc. | Info |
| M5 | `MarkPaymentDto` has `[Required]` on `HasPaid` (a `bool`). A default `bool` is `false`, so the field is always present — the `[Required]` annotation has no effect on a non-nullable bool. | `DTOs/RoundDtos.cs` | Info |

---

## 5. Conclusions and Recommended Fix Priority

| Priority | Item | Impact |
|----------|------|--------|
| P0 | **BUG-1** — Add `Npgsql.EntityFrameworkCore.PostgreSQL` NuGet package OR switch to SQLite. App cannot start without this. | App won't run |
| P0 | **BUG-2** — Move Supabase creds out of `appsettings.json`. | Security |
| P1 | **BUG-5 / MISSING-5** — Add membership check to `GetRoundsAsync`. | Data leak |
| P1 | **MISSING-1** — Add `GET api/circles/{circleId}/my-status` member home endpoint. | Core UX missing |
| P2 | **BUG-3** — Fix or remove the `{roundId}` parameter from `OpenNext` route. | Confusing API |
| P2 | **BUG-6** — Return 401 (not 403) when JWT is absent. | Correct HTTP semantics |
| P3 | **BUG-4** — Consolidate duplicate `CreatePaymentRowsForRoundAsync`. | Maintenance |
| P3 | **MISSING-3** — Wire `INotificationService` into business events. | Notification bell empty |

The core Ekub rule enforcement (fixed order, receive once, all-paid gate, state machine) is solid and correct. The primary obstacles to a running demo are the NuGet/database-driver mismatch (P0) and the missing member home endpoint (P1).
