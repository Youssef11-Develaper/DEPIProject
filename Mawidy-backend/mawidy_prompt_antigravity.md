# Mawidy (مـوعـدي) — Smart Government & Healthcare Appointment Platform
> Full Project Brief for Implementation

---

## 🎯 Project Overview

I'm building a web platform called **Mawidy**, an Egyptian digital appointment booking system that allows citizens to book appointments at any government or healthcare institution — eliminating long queues and wasted time.

### Supported Institution Types

- 🏥 Government Hospitals — checkups, lab tests, radiology, beds, surgery, emergency
- 🏦 Banks — account opening, loans, credit cards, investment certificates
- 📱 Telecom Companies — SIM replacement, plan upgrades, complaints
- 🏩 Civil Registry — national ID, birth/marriage/death certificates
- ⚖️ Courts — session booking, document submission, case tracking
- 🏛️ Government Offices — real estate registry, taxes, customs, licenses

---

## 🏗️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend Framework | ASP.NET Core 8 |
| Architecture | Clean Architecture + CQRS + MediatR |
| ORM | Entity Framework Core 8 |
| Database | SQL Server 2022 |
| Caching | Redis |
| Real-time | SignalR |
| Authentication | ASP.NET Identity + JWT (HttpOnly Cookie) |
| Validation | FluentValidation |
| Background Jobs | Hangfire |
| SMS | Vonage / Twilio |
| Email | SendGrid |
| Maps | Google Maps API |
| File Storage | Azure Blob Storage / Local |
| Logging | Serilog + Seq |
| Testing | xUnit + Moq |
| API Docs | Swagger / Scalar |
| Identity Pattern | GUID-based PKs on all tables |
| Concurrency | Optimistic Concurrency (RowVersion) |
| Soft Delete | Global EF Core Query Filters (IsDeleted) |

---

## 🧱 Solution Structure (Clean Architecture)

```
Mawidy.sln
├── src/
│   ├── Mawidy.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Events/
│   │   └── Interfaces/
│   ├── Mawidy.Application/
│   │   ├── Features/
│   │   │   ├── Appointments/     (Commands + Queries)
│   │   │   ├── Hospitals/
│   │   │   ├── Emergency/
│   │   │   ├── Search/
│   │   │   └── Reports/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Behaviors/
│   ├── Mawidy.Infrastructure/
│   │   ├── Persistence/          (DbContext + Migrations)
│   │   ├── Services/             (SMS, Email, Blob)
│   │   ├── Hubs/                 (SignalR: Queue, Beds, Emergency)
│   │   ├── Maps/                 (Google Maps + Geocoding)
│   │   └── BackgroundJobs/       (Hangfire)
│   ├── Mawidy.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   └── Mawidy.Web/
│       ├── Pages/                (Razor Pages)
│       └── wwwroot/
└── tests/
    ├── Mawidy.UnitTests/
    └── Mawidy.IntegrationTests/
```

---

## 🗄️ Database Schema (23 Tables)

All tables inherit from **BaseEntity**:

```csharp
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

---

### 1️⃣ User

```
Id            Guid        PK
NationalId    string(14)  UNIQUE | AES-256 Encrypted
FullName      string(100)
Phone         string(11)  UNIQUE
Email         string      UNIQUE
PasswordHash  string
Role          enum        (Citizen, Staff, BranchManager,
                           OrgAdmin, SuperAdmin, EmergencyUser)
IsVerified    bool
ProfilePicUrl string?
LastLoginAt   DateTime?
```

---

### 2️⃣ UserAddress

```
Id          Guid    PK
UserId      Guid    FK → User
Label       string  (Home / Work) — Multi-valued Attribute
City        string
District    string
Street      string
Latitude    decimal
Longitude   decimal
IsDefault   bool
```

---

### 3️⃣ Notification

```
Id        Guid     PK
UserId    Guid     FK → User
Title     string
Message   string
Type      enum     (Booking, Reminder, Complaint, System)
IsRead    bool
SentAt    DateTime
```

---

### 4️⃣ ServiceProvider

```
Id        Guid    PK
Name      string
NameEn    string
Type      enum    (Hospital, Bank, Telecom, Gov, Court)
LogoUrl   string?
IsActive  bool
```

---

### 5️⃣ Branch

```
Id          Guid    PK
ProviderId  Guid    FK → ServiceProvider
Name        string
Address     string
City        string
Latitude    decimal
Longitude   decimal
Phone       string  (Multi-valued Attribute)
IsActive    bool
```

---

### 6️⃣ BranchWorkingHours *(Weak Entity)*

```
Id          Guid      PK
BranchId    Guid      FK → Branch
DayOfWeek   enum
OpenTime    TimeOnly
CloseTime   TimeOnly
IsClosed    bool
```

---

### 7️⃣ Department

```
Id          Guid    PK
BranchId    Guid    FK → Branch
Name        string
Type        enum    (Medical, Financial, Civil, Legal)
IsActive    bool
```

---

### 8️⃣ Service

```
Id            Guid     PK
DepartmentId  Guid     FK → Department
Name          string
DurationMin   int
Price         decimal?
IsActive      bool
```

---

### 9️⃣ Staff

```
Id              Guid    PK
UserId          Guid    FK → User   UNIQUE
BranchId        Guid    FK → Branch
DepartmentId    Guid    FK → Department
EmployeeCode    string  UNIQUE
Title           string
Specialization  string?
IsDoctor        bool
MaxSlotsPerHour int
```

---

### 🔟 StaffSchedule

```
Id          Guid      PK
StaffId     Guid      FK → Staff
DayOfWeek   enum
StartTime   TimeOnly
EndTime     TimeOnly
IsOff       bool
```

---

### 1️⃣1️⃣ AppointmentSlot

```
Id            Guid      PK
ServiceId     Guid      FK → Service
StaffId       Guid?     FK → Staff
SlotDate      DateOnly  INDEX
StartTime     TimeOnly
EndTime       TimeOnly
MaxCapacity   int
IsAvailable   bool
```

> **Note:** No `Booked` field — COUNT query on Appointment table is used instead to prevent Race Condition.

---

### 1️⃣2️⃣ Appointment ⭐ *Core Table*

```
Id              Guid      PK
UserId          Guid      FK → User
ServiceId       Guid      FK → Service
DepartmentId    Guid      FK → Department
BranchId        Guid      FK → Branch
StaffId         Guid?     FK → Staff
SlotId          Guid      FK → AppointmentSlot
ScheduledAt     DateTime  INDEX
DurationMinutes int
Status          enum      (Pending, Confirmed, Cancelled,
                           Completed, NoShow)
QueueNumber     int
QRCode          string    UNIQUE
Notes           string?
CancelReason    string?
BookedAt        DateTime
RowVersion      byte[]    -- EF Core Optimistic Concurrency Token
```

---

### 1️⃣3️⃣ QueueTicket

```
Id              Guid      PK
AppointmentId   Guid      FK → Appointment  UNIQUE
DepartmentId    Guid      FK → Department
QueueDate       DateOnly
TicketNumber    int
IssuedAt        DateTime
CalledAt        DateTime?
ServedAt        DateTime?
Status          enum      (Waiting, Called, Served, Skipped)
```

---

### 1️⃣4️⃣ MedicalRecord

```
Id               Guid     PK
UserId           Guid     FK → User
BloodType        enum     (A+, A-, B+, B-, AB+, AB-, O+, O-)
Allergies        string?
ChronicDiseases  string?
Weight           decimal?
Height           decimal?
```

---

### 1️⃣5️⃣ MedicalFile

```
Id             Guid     PK
RecordId       Guid     FK → MedicalRecord
AppointmentId  Guid?    FK → Appointment
FileType       enum     (XRay, BloodTest, Scan, ECG, Other)
FileName       string
FileUrl        string
FileSizeMB     decimal
StorageType    enum     (Local, S3, Azure)
UploadedAt     DateTime
```

---

### 1️⃣6️⃣ Prescription

```
Id             Guid      PK
AppointmentId  Guid      FK → Appointment
DoctorId       Guid      FK → Staff
PatientId      Guid      FK → User
Notes          string?
IssuedAt       DateTime
ValidUntil     DateOnly?
```

---

### 1️⃣7️⃣ PrescriptionItem

```
Id              Guid    PK
PrescriptionId  Guid    FK → Prescription
DrugName        string
Dosage          string
Frequency       string
DurationDays    int
Instructions    string?
```

---

### 1️⃣8️⃣ Complaint

```
Id             Guid     PK
UserId         Guid     FK → User
BranchId       Guid     FK → Branch
Type           enum     (Neglect, Delay, Behavior, Billing, Other)
Description    string
Status         enum     (Pending, InProgress, Resolved, Rejected)
TrackingCode   string   UNIQUE
AttachmentUrl  string?
SubmittedAt    DateTime
ResolvedAt     DateTime?
```

---

### 1️⃣9️⃣ ComplaintReply

```
Id           Guid     PK
ComplaintId  Guid     FK → Complaint
StaffId      Guid     FK → Staff
Message      string
RepliedAt    DateTime
```

---

### 2️⃣0️⃣ Rating

```
Id                Guid     PK
AppointmentId     Guid     FK → Appointment  UNIQUE
UserId            Guid     FK → User
BranchId          Guid     FK → Branch
StaffId           Guid?    FK → Staff
OverallScore      decimal
SpeedScore        decimal?
CleanlinessScore  decimal?
StaffScore        decimal?
Comment           string?
RatedAt           DateTime
```

---

### 2️⃣1️⃣ EmergencyRequest

```
Id           Guid     PK
UserId       Guid?    FK → User   (nullable — no login required)
BranchId     Guid     FK → Branch
CallerPhone  string
Latitude     decimal
Longitude    decimal
Status       enum     (Sent, Accepted, Dispatched, Resolved)
EtaMinutes   int?
RequestedAt  DateTime
ResolvedAt   DateTime?
```

---

### 2️⃣2️⃣ Payment

```
Id             Guid     PK
AppointmentId  Guid     FK → Appointment
Amount         decimal
Method         enum     (Cash, Card, Wallet)
Status         enum     (Pending, Paid, Failed)
PaidAt         DateTime?
```

---

### 2️⃣3️⃣ AuditLog *(Append-Only)*

```
Id          Guid     PK
UserId      Guid?    FK → User
Action      string
EntityName  string
EntityId    string
OldValues   json?
NewValues   json?
IpAddress   string?
CreatedAt   DateTime INDEX
```

---

## 🔗 Key Relationships

```
User → UserAddress            1:N
User → Notification           1:N
User → Appointment            1:N
User → MedicalRecord          1:N
User → Complaint              1:N
User → AuditLog               1:N

ServiceProvider → Branch      1:N
Branch → Department           1:N
Branch → Staff                1:N
Branch → BranchWorkingHours   1:N  (7 records per branch)
Branch → Complaint            1:N

Department → Service          1:N
Service → AppointmentSlot     1:N

Staff → StaffSchedule         1:N
Staff → Appointment           1:N

AppointmentSlot → Appointment 1:N  (bounded by MaxCapacity)

Appointment → QueueTicket     1:1
Appointment → Rating          1:1
Appointment → Prescription    1:1
Appointment → Payment         1:1

Prescription → PrescriptionItem  1:N
MedicalRecord → MedicalFile      1:N
Complaint → ComplaintReply       1:N
```

---

## 👥 User Roles & Permissions

| Role | Permissions |
|------|------------|
| Citizen | Search + Book + Cancel + Track + Rate + Medical Record |
| Staff | Call queue + Confirm attendance + Postpone appointment |
| BranchManager | Manage schedules + Reports + Staff management |
| OrgAdmin | Manage all branches + Full analytics |
| SuperAdmin | Manage all organizations + System config |
| EmergencyUser | Instant ambulance request via GPS — no login required |

---

## 🔄 Booking Flow (9 Steps)

```
1. Login            → JWT Token stored in HttpOnly Cookie
2. Select Provider  → Map + List results from Redis Cache
3. Select Doctor    → Display specialization, rating, fee
4. Select Slot      → Live available times queried from DB
5. Confirm Booking  → POST /appointments
                      + Optimistic Concurrency check (RowVersion)
6. Queue Ticket     → Auto-generated QueueNumber + QR Code
7. Instant Notify   → SMS + Email dispatched via Hangfire Job
8. Reminder         → Scheduled Job fires 1 hour before → SMS
9. Attendance       → Staff scans QR → SignalR updates waiting screen
```

---

## 🚑 Emergency Flow

```
1. User presses Emergency button (available on every page)
2. Geolocation API captures GPS coordinates (lat/lng) automatically
3. POST /emergency → system finds nearest hospital with available ambulance
4. SignalR instantly notifies hospital with location + description
5. User receives ambulance phone number + ETA in minutes
6. Real-time ambulance tracking shown on user's map

-- No login required
-- Rate limited to prevent abuse
-- IP address logged for post-incident verification
-- Must use Direct API Call (NOT background job) for instant delivery
```

---

## 📡 Key API Endpoints

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| POST | /api/v1/auth/register | Public | Register new citizen |
| POST | /api/v1/auth/login | Public | Login + issue JWT |
| GET | /api/v1/organizations | Public | List all providers with filter |
| GET | /api/v1/branches/nearby | Public | Nearest branches by GPS |
| GET | /api/v1/doctors/available | Public | Available doctors |
| POST | /api/v1/appointments | Citizen | Create new appointment |
| GET | /api/v1/appointments/my | Citizen | My appointments list |
| DELETE | /api/v1/appointments/{id} | Citizen | Cancel appointment |
| GET | /api/v1/beds/available | Public | Live available hospital beds |
| POST | /api/v1/bed-bookings | Citizen | Book a hospital bed |
| POST | /api/v1/surgeries | Doctor | Schedule a surgery |
| POST | /api/v1/lab-orders | Doctor | Request lab test |
| POST | /api/v1/radiology-orders | Doctor | Request radiology scan |
| POST | /api/v1/emergency | Public | Instant ambulance request |
| GET | /api/v1/queue/{branchId}/today | Staff | Today's queue |
| PUT | /api/v1/appointments/{id}/call | Staff | Call next patient |
| GET | /api/v1/reports/branch/{id} | Manager | Branch performance report |
| GET | /api/v1/reports/occupancy/{id} | Manager | Hospital occupancy report |

---

## ⚡ Advanced EF Core Patterns

### 1. Optimistic Concurrency — Race Condition Prevention

```csharp
// RowVersion on Appointment table
// Scenario: 2 users book the same slot simultaneously
// → First commit wins
// → Second user gets DbUpdateConcurrencyException
// → Application retries with fresh slot availability check

modelBuilder.Entity<Appointment>()
    .Property(a => a.RowVersion)
    .IsRowVersion();
```

### 2. Global Query Filter — Soft Delete

```csharp
// Applied automatically to ALL queries — no manual .Where() needed
modelBuilder.Entity<Appointment>()
    .HasQueryFilter(a => !a.IsDeleted);

// To bypass filter when needed (e.g., Admin restore):
dbContext.Appointments.IgnoreQueryFilters().Where(...)
```

### 3. Value Converter — NationalId AES-256 Encryption

```csharp
modelBuilder.Entity<User>()
    .Property(u => u.NationalId)
    .HasConversion(new AesEncryptionConverter(encryptionKey));
// Encryption/decryption is transparent to the application layer
```

### 4. Indexes for Performance

```csharp
// Applied on fields used in frequent queries
modelBuilder.Entity<AppointmentSlot>()
    .HasIndex(s => s.SlotDate);

modelBuilder.Entity<Appointment>()
    .HasIndex(a => a.ScheduledAt);

modelBuilder.Entity<AuditLog>()
    .HasIndex(l => l.CreatedAt);

// Unique indexes (also enforce business rules)
modelBuilder.Entity<User>()
    .HasIndex(u => u.NationalId).IsUnique();
modelBuilder.Entity<User>()
    .HasIndex(u => u.Phone).IsUnique();
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email).IsUnique();
```

---

## 🔒 Security Implementation

| Threat | Solution |
|--------|----------|
| SQL Injection | EF Core Parameterized Queries — no Raw SQL allowed |
| CSRF | AntiForgery Token + SameSite Strict Cookie |
| Brute Force | Rate Limiting: 5 attempts/min + Account Lockout |
| Unauthorized API | JWT Bearer + Policy-based Authorization per Role |
| Medical Data Leak | NationalId + Medical Records encrypted AES-256 |
| Race Condition | Optimistic Concurrency (RowVersion) on Appointment |
| File Upload Abuse | MIME Type validation + size limit + Virus Scan |
| Emergency Abuse | Rate Limiting + IP logging + post-verification |
| Data Exposure | DTOs only in API responses — never raw Entities |
| Log Leak | NationalId and Phone auto-masked in Serilog Enricher |

---

## 🎨 Frontend — Already Designed

The UI is complete as standalone self-contained HTML files. The backend needs to be connected to these pages.

### Design System

| Property | Value |
|----------|-------|
| Primary Font | Tajawal + Cairo (Google Fonts) |
| Direction | RTL (Right-to-Left — Arabic first) |
| Navy | `#0B1F3A` |
| Blue | `#1565C0` |
| Teal | `#00897B` |
| Red | `#E53935` |
| Gold | `#F9A825` |

### Completed Pages

**1. Main Dashboard** — `mawidy-ui (3).html`

Full platform UI including:
- Hero section with live stats
- Problem/Solution section
- 6-service grid with detail modals (sub-services, branches, required docs)
- Mobile app mockups (5 screens: Home, Search/Map, Booking, My Appointments, Emergency)
- 4-step booking wizard modal
- Complaints system with department tabs and live list
- Stats dashboard with bar chart + donut chart
- Heatmap section with heat spots and area ranking
- Hospital features tab (medical records, digital prescriptions, live beds grid, file upload)
- Rating & reviews system with score bars
- Emergency modal with real-time ETA countdown simulation
- AI chatbot widget with quick replies
- Notification center panel
- Login/Register overlay modals with role switching

**2. Courts Service Page** — `mawidy-courts.html`

- Gold/Navy judicial aesthetic (`--court: #B8860B`)
- 9 court services with booking flow
- Court directory with type filtering
- Case tracking with timeline steps
- Required documents section by service type
- FAQ accordion grid
- Full booking form with time slot grid
- Live queue sidebar with real-time counter
- Case tracking with example data

**3. UI Laws / Design Reference** — `Mawidy_UI_laws_V1.html`

Design tokens and component reference

### Frontend Tech

- Vanilla HTML / CSS / JavaScript — zero external JS dependencies
- CSS Custom Properties for theming across all pages
- Intersection Observer API for scroll animations
- SignalR hooks prepared (real-time slots ready for backend connection)
- All pages are fully self-contained single files

---

## 📊 ERD Overview

The ERD uses **Chen Notation** (draw.io format) organized into 6 domain groups:

| Group | Tables |
|-------|--------|
| Identity & Users | User, UserAddress, Staff, StaffSchedule |
| Providers & Services | ServiceProvider, Branch, BranchWorkingHours, Department, Service, AppointmentSlot |
| Appointments & Queue | Appointment, QueueTicket, Payment |
| Healthcare | MedicalRecord, MedicalFile, Prescription, PrescriptionItem |
| Notifications & Ratings | Notification, Rating, Complaint, ComplaintReply |
| Emergency & Security | EmergencyRequest, AuditLog |

ERD file: `ERD_mawidy.html` (draw.io — Chen Notation — 23 entities fully mapped)

---

## 📋 Documentation Already Produced

### `mawidy_architecture_v2.docx`
- Full platform vision and problem/solution framing
- Complete service catalog (hospital + all government types)
- Clean Architecture layer-by-layer breakdown
- Full tech stack with justification for each choice
- Complete API endpoints reference
- Security measures and threat model
- 6-phase development roadmap with durations
- Best practices and production recommendations

### `mawidy_erd_explained.docx`
- Detailed explanation of all 23 tables
- Every field documented with type, constraints, and business purpose
- All relationships with cardinality explanation and reasoning
- Advanced EF Core concepts explained in context
- Weak entities and multi-valued attributes explanation
- Practical implementation tips for each table

---

## 🚀 Development Roadmap

| Phase | Duration | Deliverables |
|-------|----------|-------------|
| 1 — Foundation | 2 weeks | Clean Architecture setup + DB Schema + Auth + Identity |
| 2 — Core Booking | 3 weeks | Appointment flow + Digital queue + Staff dashboard |
| 3 — Hospital | 3 weeks | Bed booking + Medical records + Radiology/Labs + Doctor dashboard |
| 4 — Maps & Search | 2 weeks | Google Maps integration + Geo search + Filters |
| 5 — Emergency | 1 week | Emergency system + SignalR real-time + Instant SMS |
| 6 — Polish | 2 weeks | Reports + Testing + Security audit + Deployment + CI/CD |

---

## ⚠️ Critical Implementation Rules

1. **Start with a Vertical Slice** — Complete one full feature (medical checkup booking) from UI to DB before starting the next feature.

2. **Concurrency is non-negotiable** — Use Optimistic Concurrency (RowVersion) on `AppointmentSlot`. Track bookings via COUNT on `Appointment` table, never store a `Booked` counter field.

3. **Emergency endpoint must be Stateless** — It must work even if the Auth Service goes down. No authentication dependency.

4. **Medical data requires Encryption at Rest** — AES-256 on NationalId and sensitive medical fields. Audit Log on every read/write access.

5. **Google Maps API has billing** — Cache all Geocoding results in Redis to minimize API calls and control costs.

6. **Pagination from day one** — Add `PageNumber` + `PageSize` to all List endpoints before any data grows.

7. **Emergency SMS = Direct API Call** — Must NOT go through Hangfire background queue. Instant delivery is a safety requirement.

8. **AuditLog is Append-Only** — Block `UPDATE` and `DELETE` at the Repository level. No exceptions.

9. **NationalId must be encrypted before DB insert** — Use EF Core `Value Converter` with AES-256. Never store plaintext.

10. **Global Query Filter for IsDeleted** — Configure it in `DbContext.OnModelCreating()` from day one so it applies everywhere automatically.

---

## 📁 Project Files Reference

| File | Description |
|------|-------------|
| `mawidy-ui (3).html` | Main platform UI — complete dashboard with all features |
| `mawidy-courts.html` | Courts service page — fully built |
| `Mawidy_UI_laws_V1.html` | UI design reference and component library |
| `ERD_mawidy.html` | Full ERD in draw.io format (Chen Notation, 23 entities) |
| `mawidy_architecture_v2.docx` | Architecture documentation v2.0 |
| `mawidy_erd_explained.docx` | Detailed ERD explanation for all tables |
| `New_Text_Document.txt` | Raw DB schema reference (all 23 tables) |

---

## 🎯 What Needs to Be Built

The **frontend UI is complete**. The backend needs to be built from scratch following this architecture. Specifically:

1. Set up the Clean Architecture solution structure (5 projects)
2. Implement all 23 Domain Entities with full EF Core configurations
3. Configure DbContext with:
   - Global Query Filters (Soft Delete on all entities)
   - Optimistic Concurrency Token (RowVersion on Appointment)
   - Value Converters (NationalId AES-256 encryption/decryption)
   - All required Indexes
4. Implement CQRS pattern with MediatR (Commands + Queries + Handlers)
5. Build the core Appointment booking flow end-to-end
6. Set up SignalR hubs: QueueHub, BedHub, EmergencyHub
7. Implement the stateless Emergency endpoint (no auth dependency)
8. Connect the existing HTML frontend to the REST API
9. Configure JWT authentication with role-based authorization policies
10. Set up Hangfire background jobs for SMS reminders and email notifications

---

*Platform Target: Egypt 🇪🇬 — Language: Arabic RTL — Backend code: English*
*Mawidy v2.0 — ASP.NET Core 8 — Clean Architecture — Production Ready*
