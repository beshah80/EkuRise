# EkuRise — Digital Rotating Savings Circle (Ekub) Ledger

## Team Members & Responsibilities

- Lidiya Kebede — Team Lead & Pitch
- Beza Hailemariam — Frontend Developer
- Liyuwerk Kebede — Frontend Developer
- Lidiya Kebede — Backend Developer
- Beshah Ashenafi — Backend Developer

---

## Tech Stack

- Frontend: Angular 17 with TypeScript
- Backend: ASP.NET Core Web API (.NET 10)
- Database: PostgreSQl
- ORM: Entity Framework Core
- Authentication: JWT Bearer Token, Phone OTP, and PIN Login

---

## How to Run Locally

### Prerequisites

Make sure you have installed:

- Node.js
- Angular CLI
- .NET SDK
- Entity Framework Core CLI

### Backend

```bash
cd backend/EkubApi
dotnet restore
dotnet ef database update
dotnet run

## Test Accounts & Demo Credentials

| Name | Phone | PIN | Role |
|---|---|---|---|
| Admin Organizer | 0912345678 | 1234 | Admin |
| Standard Member | 0987654321 | 1234 | Member |
| Abeba Tadesse | 0911112222 | 1234 | Member |
| Kidane Bekele | 0922223333 | 1234 | Member |
| Dawit Haile | 0933334444 | 1234 | Member |
| Sara Girma | 0944445555 | 1234 | Member |

---

## What is EkubCircle?

EkubCircle is a digital ledger for the traditional Ethiopian rotating savings circle called **Ekub**. In a traditional Ekub, a group of people agree on a fixed contribution amount. Every meeting, all members pay into the pot and one member takes the full pot. The rotation continues until every member has received once.

EkubCircle brings this centuries-old system into a mobile-first digital platform. It is a **record book, not a bank** — no real money moves through the app. The organizer records payments and payouts. The system enforces all Ekub rules automatically.

---

## Working Features

### Authentication
- Phone number-based registration (no email required)
- Full profile at registration: first name, last name, gender, job type, location
- OTP verification via phone (demo code shown in response for testing)
- PIN login for returning users (fast, no OTP needed)
- JWT token-based session management
- Referral code system — every user gets a unique code like `EK-ABTA-1003`
- Preferred language setting: English or Amharic

### Public Ekub Catalog (Home Page)
- Admin creates main categories: Driver Equb, Trader Equb, Workers Equb, Ye Ayinet Equb, Farmers Equb
- Under each category, admin creates specific Ekub plans (sub-categories) with:
  - Daily contribution amount (e.g., 300 ETB)
  - Total rounds (e.g., 105 rounds = 105 members)
  - Total pot amount (e.g., 31,500 ETB per winner)
  - Scheduled start date
  - Custom terms and conditions text
  - Maximum member slots
- Users browse the catalog on the home page with category filter chips
- Each Ekub card shows: daily amount, rounds, total pot, start date, member progress bar, status badge
- Users tap a card to see the full detail including terms and conditions
- Users must check "I agree to the terms and conditions" before joining
- Join button is disabled if the Ekub is full, already started, or already joined
- When max members is reached, status changes to "Full" automatically

### Circle Management (Private Circles)
- Any user can create their own private circle with a name, contribution amount, and meeting label
- **Forming state:** organizer adds members by phone number, removes members, sees the list
- Start Circle: locks the member list, assigns fixed payout order (by join time), creates one round per member, opens the first round
- **Active state:** rounds run one by one, organizer manages payments and payouts
- **Completed state:** circle summary shows who received which round and when

### Round Management
- Current round card shows: round number out of total, pot amount, payment progress bar
- Organizer sees payment toggles for each member (checkbox to mark paid/unpaid)
- Pot updates in real time as payments are marked (paid count × contribution)
- Pay Out button is only enabled when every single member is marked paid
- Payout goes to the next member in the fixed order — not a typed name
- After payout, organizer sees "Open Next Round" button
- Round history with filter by round number and status (Open / Paid Out)

### Member View
- Members see: their payment status this round (Paid ✓ / Unpaid)
- Members see: whether they have received the pot yet (Yes / Not yet)
- Members see: the payout order list showing all members with their position
- Members see: full round history — who won each round, when, for how much
- Members cannot toggle payments or trigger payouts (read-only)

### Notifications
- Bell icon on bottom nav with unread count badge
- Notification types: payment reminder, payout received, round opened, member joined, circle started, circle completed
- Mark individual notification as read or mark all read
- Tap a notification to navigate directly to the related circle or round

### Account Page
- Profile header with avatar, full name, phone, job type, location
- Edit profile: update name, email, gender, job type, location
- **Privacy & Security:**
  - Enable or disable PIN login
  - Change PIN
  - Delete account with phone OTP verification (sends a code, user must confirm)
- **Language:** switch between English and Amharic
- **My Ekubs / History:** list of all Ekubs the user has joined, with status and link to circle
- **Questions & Feedback:** submit questions, see response status (Pending / Reviewed)
- **Success Stories:** read approved testimonials from other Ekub participants, submit your own story with a 1–5 star rating
- **Referral:** view and copy personal referral code, see how many people you referred
- **About Us:** information about EkubCircle
- Logout

### Admin Features (Admin account only)
- Manage Catalog page accessible from Account menu
- Create new main categories
- Create sub-categories with full details and terms and conditions
- See how many members have joined each sub-category
- Start an Ekub — auto-creates a Circle from all joined members, sets payout order, opens Round 1
- View the auto-created Circle

---

## Hard Rules Enforced by the Server

These rules cannot be bypassed from the frontend — the API enforces them:

1. **Payout requires all members paid** — if even one member is unpaid, the API returns 400 with the names of unpaid members
2. **Fixed payout order** — the receiver of each round is determined by their position in the order set at Start, never by typing a name
3. **Receive once only** — a member can receive the pot at most once per circle. A second payout attempt to the same member returns 400
4. **No double payout** — paying out a round that is already paid out returns 400
5. **No joining twice** — a user cannot join the same Ekub sub-category twice, returns 400
6. **Members still pay after receiving** — a member who has received the pot stays on the payment list and must continue paying for remaining rounds
7. **Start requires minimum 2 members** — the Start button is disabled and the API rejects it with fewer than 2 members
8. **Forming-only operations** — adding/removing members and starting the circle is only possible while status is Forming

---

## Database — 13 Tables

| Table | Purpose |
|---|---|
| Users | Phone-based profiles, PIN, referral code, language, admin flag |
| PhoneVerifications | OTP codes for registration and login (6-digit, 10-min expiry) |
| EkubCategories | Admin-created main categories (Driver Equb, Trader Equb, etc.) |
| EkubSubCategories | Specific Ekub plans with contribution, rounds, total, start date, T&C |
| EkubSubscriptions | Records user joining a sub-category after agreeing to T&C |
| Circles | Private rotating savings groups (Forming → Active → Completed) |
| CircleMembers | Members with fixed payout order and received flag |
| Rounds | One per member (Pending → Open → PaidOut) |
| Payments | One per member per round — paid status, paid date, late fine |
| Notifications | All app notifications with type, read status, circle/round links |
| Feedbacks | User questions and support messages |
| SuccessStories | Member testimonials (admin approval required before visible) |
| AccountDeletionRequests | Deletion requests with OTP confirmation |

---

## API Endpoints

### Authentication
```
POST /api/auth/register              Register new user, returns OTP
POST /api/auth/register/verify       Verify OTP, activate account, returns JWT
POST /api/auth/login/otp             Send login OTP to phone
POST /api/auth/login/verify          Verify login OTP, returns JWT
POST /api/auth/login/pin             Login with PIN, returns JWT
GET  /api/auth/me                    Get current user profile
PUT  /api/auth/me                    Update profile
POST /api/auth/me/pin                Set or change PIN
POST /api/auth/delete-account/request   Request deletion OTP
POST /api/auth/delete-account/confirm   Confirm deletion with OTP
```

### Catalog
```
GET  /api/catalog/categories                          List all categories
POST /api/catalog/categories                          Create category (admin)
GET  /api/catalog/categories/{id}/subcategories       List sub-categories
GET  /api/catalog/subcategories/{id}                  Sub-category detail + T&C
POST /api/catalog/subcategories                       Create sub-category (admin)
POST /api/catalog/subcategories/{id}/join             Join with T&C agreement
POST /api/catalog/subcategories/{id}/start            Start Ekub (admin)
GET  /api/catalog/my-ekubs                            My joined Ekubs
```

### Circles
```
POST   /api/circles                          Create circle
GET    /api/circles                          My circles
GET    /api/circles/{id}                     Circle detail
POST   /api/circles/{id}/members             Add member by phone
DELETE /api/circles/{id}/members/{userId}    Remove member
POST   /api/circles/{id}/start               Start circle
```

### Rounds
```
GET  /api/circles/{id}/rounds                           Round history (filterable)
GET  /api/circles/{id}/rounds/current                   Current open round
GET  /api/circles/{id}/rounds/{roundId}                 Round detail
PUT  /api/circles/{id}/rounds/{roundId}/payments        Mark member paid/unpaid
POST /api/circles/{id}/rounds/{roundId}/payout          Pay out the pot
POST /api/circles/{id}/rounds/{roundId}/next            Open next round
```

### Notifications
```
GET /api/notifications               All notifications
GET /api/notifications/unread-count  Unread count
PUT /api/notifications/{id}/read     Mark one read
PUT /api/notifications/read-all      Mark all read
```

### Feedback & Stories
```
POST /api/feedback           Submit feedback
GET  /api/feedback           My feedbacks
GET  /api/success-stories    Approved success stories
POST /api/success-stories    Submit success story
```

---

## Project Structure

```
hackaton/
├── backend/
│   └── EkubApi/
│       ├── Controllers/      Slim controllers, delegate to services
│       ├── Services/         All business logic lives here
│       ├── DTOs/             Request/response models
│       ├── Entities/         EF Core entity classes
│       ├── Data/             DbContext with all configurations
│       ├── Enums/            Shared enumerations
│       ├── Middleware/       Global exception handler
│       ├── Program.cs        DI, JWT, CORS, auto-migrate, seed
│       └── appsettings.json  Connection string, JWT settings
└── frontend/
    └── src/app/
        ├── components/       One folder per screen
        ├── services/         API service classes
        ├── guards/           AuthGuard, AdminGuard
        ├── interceptors/     JWT attachment
        └── models/           TypeScript interfaces matching DTOs
```

---

## Known Limitations & Bugs

- OTP codes are returned in the API response (demo only — in production would be sent via SMS)
- No real SMS integration (Telebirr, Ethio Telecom) — this is intentional per challenge rules
- Profile picture upload stores a URL string only — no actual file upload implemented
- Fingerprint login is a UI toggle only — no Web Authentication API integration
- Amharic language toggle is stored but UI translation not fully implemented
- Admin approval for success stories must be done directly in the database for the demo
- No push notifications — notifications are pulled from the API on page load

---

## Extra Credit Features Implemented

- **Late fine field** on payment rows — organizer can record a fine amount per member per round
- **Referral system** — every user gets a unique referral code, can track who they referred
- **Ekub catalog** — admin-managed public Ekub catalog with categories and sub-categories
- **Terms and conditions** per sub-category — users must agree before joining
- **Completed circle summary** — full table of who received which round and when
