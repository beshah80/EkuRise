# EnkuRise — Project Guide & Demo Defense

## What This App Is

EnkuRise is a digital ledger for a traditional Ethiopian rotating savings group (Ekub). A group of people agree on a fixed contribution amount. Every round, everyone pays into a shared pot, and one member takes the whole pot. The circle ends when every member has received once. This app replaces the notebook and Telegram group with a proper record-keeping system.
dotnet run 
Using launch settings from D:\Projects\hackaton\backend\EkubApi\Properties\launchSettings.json...
Building...
[DB CONNECT] Configured connection string test failed: The operation has timed out
[DB CONNECT] Tried eu-west-1:5432 -> The operation has timed out
[DB CONNECT] Tried eu-west-1:6543 -> Failed to connect to 34.241.16.247:6543
[DB CONNECT] Tried eu-west-2:5432 -> The operation has timed out
[DB CONNECT] Tried eu-west-2:6543 -> The operation has timed out
[DB CONNECT] Tried eu-west-3:5432 -> The operation has timed out
[DB CONNECT] Tried eu-west-3:6543 -> Failed to connect to 13.39.246.141:6543


**This is not a bank. No real money moves through the app. It is a record book.**

---

## The Two Roles

### Member
A regular participant in the circle.

- Signs up with phone number and PIN
- Sees their circle: contribution amount, meeting label, member list
- Sees the current round: who has paid, who hasn't, pot so far
- Sees their own status: did I pay this round? Have I received my pot yet?
- Sees who won past rounds in order
- Sees their position in the payout queue

### Organizer
The person who runs the circle. They are also a member — they pay and can receive just like everyone else. The only difference is they have admin controls.

- Creates the circle (name, contribution in Birr, meeting label)
- Adds members while the circle is forming
- Starts the circle (locks the member list, sets payout order)
- Marks who paid each round
- Pays out the pot to the rightful member
- Opens the next round

---

## How the App Works — Step by Step

### Phase 1: Forming
1. Organizer registers and creates a circle with a name, contribution amount (e.g. 500 ETB), and meeting label (Weekly / Monthly)
2. Organizer adds members one by one using their phone number — each member must already be registered
3. The circle shows all members in a list
4. When ready, organizer clicks **Start Circle**

### Phase 2: Active
Starting the circle does three things automatically on the server:
- Locks the member list — no one can be added or removed after this
- Creates one round per member (5 members = 5 rounds)
- Assigns a fixed payout order based on join order (member 1 wins round 1, member 2 wins round 2, etc.)
- Opens round 1 immediately

Every round follows this flow:

```
Members pay → Organizer marks each as paid → All paid → Organizer pays out → Winner determined by fixed order → Organizer opens next round
```

### Phase 3: Completed
When every member has received the pot once, the circle is automatically marked Completed. A summary table shows who received which round, how much, and when.

---

## The Hard Rules (Enforced on the Server)

These are not UI tricks — the server blocks them with HTTP 400 errors:

| Rule | What happens if violated |
|------|--------------------------|
| Payout before all members paid | 400 — lists who hasn't paid |
| Same member receives twice | 400 — "already received the pot" |
| Payout on an already paid-out round | 400 — "round already paid out" |
| Non-member accessing circle data | 404 — not found |

**The winner is never typed in by the organizer.** The server picks the next person in the fixed queue automatically. Round 1 winner = position 1, Round 2 winner = position 2, and so on.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Angular 19 with TypeScript, standalone components, reactive forms, route guards |
| Backend | ASP.NET Core Web API, .NET 10, C# |
| Database | PostgreSQL on Supabase via Entity Framework Core 10 |
| Auth | JWT Bearer tokens, phone + PIN login, OTP verification |
| Deployment | Backend on Railway, Frontend on Vercel |

---

## Architecture

```
Angular (Vercel)
    ↓ HTTP + JWT
ASP.NET Core API (Railway)
    ↓ EF Core
PostgreSQL (Supabase)
```

**Controllers are slim** — no business logic inside them. All logic lives in service classes:
- `CircleService` — create circle, add members, start circle, member home
- `RoundService` — mark payment, payout, open next round, round history
- `AuthService` — register, OTP, PIN login, JWT generation

**DTOs** are used everywhere — raw EF Core entities never reach the frontend.

---

## API Endpoints

### Auth
| Method | Route | What it does |
|--------|-------|--------------|
| POST | `/api/auth/register` | Register with phone, get OTP |
| POST | `/api/auth/register/verify` | Verify OTP, get token |
| POST | `/api/auth/login/pin` | Login with phone + PIN |
| GET | `/api/auth/me` | Get own profile |

### Circles
| Method | Route | What it does |
|--------|-------|--------------|
| POST | `/api/circles` | Create a circle |
| GET | `/api/circles` | My circles |
| GET | `/api/circles/{id}` | Circle detail (members only) |
| POST | `/api/circles/{id}/members` | Add member (organizer, forming only) |
| DELETE | `/api/circles/{id}/members/{userId}` | Remove member (organizer, forming only) |
| POST | `/api/circles/{id}/start` | Start circle (organizer only) |
| GET | `/api/circles/{id}/my-status` | My payment status, pot, winner history |

### Rounds
| Method | Route | What it does |
|--------|-------|--------------|
| GET | `/api/circles/{id}/rounds` | Round history with filter |
| GET | `/api/circles/{id}/rounds/current` | Current open round |
| GET | `/api/circles/{id}/rounds/{roundId}` | Specific round detail |
| PUT | `/api/circles/{id}/rounds/{roundId}/payments` | Mark member paid (organizer) |
| POST | `/api/circles/{id}/rounds/{roundId}/payout` | Pay out the round (organizer) |
| POST | `/api/circles/{id}/rounds/next` | Open next round (organizer) |

---

## Database Schema

```
Users
  Id, PhoneNumber, FirstName, LastName, PinHash, IsAdmin, ...

Circles
  Id, Name, Contribution, MeetingLabel, Status (Forming/Active/Completed)
  OrganizerId → Users

CircleMembers
  Id, CircleId, UserId, PayoutOrder, HasReceived, JoinedAt
  Unique index on (CircleId, UserId)

Rounds
  Id, CircleId, RoundNumber, Status (Pending/Open/PaidOut)
  ReceiverId → Users, OpenedAt, PaidOutAt
  Unique index on (CircleId, RoundNumber)

Payments
  Id, RoundId, UserId, HasPaid, PaidAt, LateFine
  Unique index on (RoundId, UserId)
```

---

## Demo Credentials

All seed users have PIN: **1234**

| Phone | Name | Role |
|-------|------|------|
| 0912345678 | Admin Organizer | Organizer |
| 0987654321 | Standard Member | Member |
| 0911112222 | Abeba Tadesse | Member |
| 0922223333 | Kidane Bekele | Member |
| 0933334444 | Dawit Haile | Member |
| 0944445555 | Sara Girma | Member |

**Login flow:** Enter phone → receive OTP code (shown in response as `demoCode` for demo) → verify → set PIN → use PIN next time

---

## Demo Script (For the Live Walkthrough)

**Step 1 — Login (30 sec)**
- Log in as Admin Organizer: `0912345678`, PIN `1234`

**Step 2 — Create a Circle (1 min)**
- Click "Create Circle"
- Name: "Demo Ekub", Contribution: 500, Meeting: Weekly
- Add 3 members by phone: `0987654321`, `0911112222`, `0922223333`
- Click "Start Circle"
- Show the payout order is now locked: #1 Admin, #2 Standard, #3 Abeba, #4 Kidane

**Step 3 — Run Round 1 (1 min)**
- Open the circle → see Round 1 is open, winner shown as Admin Organizer
- Click "View Round Details"
- Mark all 4 members as paid one by one
- Show pot = 4 × 500 = 2,000 ETB
- Click "Pay Out to Next Member" → confirm
- Show success: "Round 1 paid out to Admin Organizer — 2,000 ETB 🎉"

**Step 4 — Open Round 2 (30 sec)**
- Click "Open Next Round"
- Show Round 2 is open, next winner is Standard Member

**Step 5 — Member View (30 sec)**
- Log in as Standard Member: `0987654321`, PIN `1234`
- Open the circle
- Show "My Status": Unpaid this round, Not received yet, Position #2
- Show Past Winners: Round 1 → Admin Organizer

**Step 6 — Prove the Hard Rule (30 sec)**
- Log back in as Organizer
- Try to pay out Round 1 again → show 400 error "round already paid out"

---

## Questions Judges May Ask — and How to Answer

**"Where is the business logic? Show me."**
> It's in the service layer. `PayOutAsync` in `RoundService.cs` enforces all three hard rules — checks if the round is already paid out, checks if all members have paid, and looks up the receiver by `PayoutOrder == RoundNumber`. The controller just calls the service and returns the result.

**"How does the system determine the winner?"**
> When the organizer starts the circle, `StartCircleAsync` assigns each member a `PayoutOrder` number (1, 2, 3...) based on join order. When payout happens for round N, the server queries `CircleMembers` where `PayoutOrder == RoundNumber`. The organizer cannot type a name — the server picks automatically.

**"What happens if the organizer tries to pay out before everyone has paid?"**
> `PayOutAsync` queries all payment rows for the round, filters where `HasPaid == false`, and if any exist it throws an `InvalidOperationException` with the unpaid members' names. The middleware maps that to HTTP 400. Angular shows the error from `err.error.title`.

**"How does authentication work?"**
> Users register with phone number and get an OTP. After verifying, they set a PIN. The PIN is hashed with PBKDF2-SHA256 (100,000 iterations, random salt). Login generates a JWT signed with a secret key. The JWT contains the user's ID as a claim. Every protected endpoint calls `GetUserId()` on the base controller which reads that claim.

**"How does member-only access work?"**
> Every circle endpoint checks the `CircleMembers` table for `CircleId == X AND UserId == currentUser`. If the user isn't a member, they get 404 back. This happens in `GetCircleByIdAsync`, `GetCurrentRoundAsync`, `GetRoundByIdAsync`, and `GetRoundsAsync`.

**"What is the circle state machine?"**
> Three states: `Forming` (0) → `Active` (1) → `Completed` (2). You can only add/remove members while Forming. You can only run rounds while Active. The circle moves to Completed automatically when every `CircleMember.HasReceived == true` after a payout.

**"What DTOs do you use?"**
> We never expose EF Core entities directly. Outgoing: `CircleDetailDto`, `RoundDetailDto`, `PaymentDto`, `MemberHomeDto`, `PayoutResultDto`. Incoming: `CreateCircleDto`, `AddMemberDto`, `MarkPaymentDto`. All in the `DTOs/` folder.

**"What HTTP status codes do you return?"**
> 201 Created for new circles, 200 OK for reads and updates, 400 Bad Request for business rule violations, 401 for missing token, 403 for wrong role, 404 for not found or non-member access.

**"How does the Angular frontend connect to the backend?"**
> Via `HttpClient` in Angular services. Every service uses `environment.apiUrl` as the base URL. The `authInterceptor` automatically attaches the JWT token as a `Bearer` header to every request except register and login. Route guards (`authGuard`, `adminGuard`) protect pages that require login.

**"Show me the database schema."**
> Five tables: `Users`, `Circles`, `CircleMembers`, `Rounds`, `Payments`. `CircleMembers` is the join table between Users and Circles, and it stores `PayoutOrder` and `HasReceived`. `Rounds` stores `RoundNumber` and `ReceiverId`. `Payments` has a unique index on `(RoundId, UserId)` so you can't pay the same person twice in a round.

---

## Known Limitations

- No real OTP SMS — the OTP code is returned in the API response as `demoCode` for demo purposes
- No push notifications — the notification bell exists but is not wired to business events
- One circle per demo — the spec says one circle MVP
- Late fine is recorded on the payment row but not automatically enforced

---

## What We Would Add With More Time

- SMS OTP via Twilio or Africa's Talking
- Push notifications when a round is paid out
- Server-side lottery draw for extra credit
- PDF summary of a completed circle
- Mobile-responsive polish
