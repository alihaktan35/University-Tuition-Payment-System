# Entity Relationship Diagram

## University Tuition Payment System - Data Model

### Visual ER Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         STUDENT ENTITY                                  │
│─────────────────────────────────────────────────────────────────────────│
│ Student                                                                  │
│ ┌─────────────────┐                                                     │
│ │ PK: Id          │                                                     │
│ │ UK: StudentNo   │                                                     │
│ │    Name         │                                                     │
│ │    Email        │                                                     │
│ │    CreatedAt    │                                                     │
│ └─────────────────┘                                                     │
│         │                                                               │
│         │ 1:N                                                           │
│         ├──────────────────┬───────────────────────────┐               │
│         │                  │                           │               │
│         ▼                  ▼                           │               │
│ ┌─────────────────┐  ┌─────────────────┐             │               │
│ │   TUITION       │  │    PAYMENT      │             │               │
│ ├─────────────────┤  ├─────────────────┤             │               │
│ │ PK: Id          │  │ PK: Id          │             │               │
│ │ FK: StudentId   │  │ FK: TuitionId   │◄────────────┘               │
│ │    StudentNo    │  │ FK: StudentId   │   1:N                       │
│ │ UK: (StudentNo, │  │    StudentNo    │                             │
│ │      Term)      │  │    Term         │                             │
│ │    Amount       │  │    Amount       │                             │
│ │    Balance      │  │    Status       │                             │
│ │    IsPaid       │  │    PaymentDate  │                             │
│ │    CreatedAt    │  │    ErrorMessage │                             │
│ │    PaidAt       │  └─────────────────┘                             │
│ └─────────────────┘                                                   │
└───────────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────────┐
│                      AUTHENTICATION & AUDIT ENTITIES                   │
│───────────────────────────────────────────────────────────────────────│
│                                                                        │
│  ┌─────────────────┐      ┌─────────────────┐                       │
│  │      USER       │      │   RATE LIMIT    │                       │
│  ├─────────────────┤      ├─────────────────┤                       │
│  │ PK: Id          │      │ PK: Id          │                       │
│  │ UK: Username    │      │    StudentNo    │                       │
│  │    PasswordHash │      │    Endpoint     │                       │
│  │    Role         │      │    RequestDate  │                       │
│  │    CreatedAt    │      │    RequestCount │                       │
│  └─────────────────┘      └─────────────────┘                       │
│                                                                        │
│  ┌──────────────────────────────────────────┐                        │
│  │              API LOG                      │                        │
│  ├──────────────────────────────────────────┤                        │
│  │ PK: Id                                    │                        │
│  │    HttpMethod                             │                        │
│  │    RequestPath                            │                        │
│  │    RequestTimestamp                       │                        │
│  │    SourceIpAddress                        │                        │
│  │    RequestHeaders                         │                        │
│  │    RequestSize                            │                        │
│  │    AuthenticationSucceeded                │                        │
│  │    StatusCode                             │                        │
│  │    ResponseLatencyMs                      │                        │
│  │    ResponseSize                           │                        │
│  │    ErrorMessage                           │                        │
│  └──────────────────────────────────────────┘                        │
└───────────────────────────────────────────────────────────────────────┘
```

### Relationships

#### Student ↔ Tuition (1:N)
- One student can have multiple tuition records (one per term)
- Each tuition record belongs to exactly one student
- Foreign Key: Tuition.StudentId → Student.Id
- Cascade delete: When student is deleted, all tuitions are deleted

#### Student ↔ Payment (1:N)
- One student can make multiple payments
- Each payment is made by exactly one student
- Foreign Key: Payment.StudentId → Student.Id
- No cascade delete to preserve payment history

#### Tuition ↔ Payment (1:N)
- One tuition can have multiple payments (partial payments)
- Each payment is for exactly one tuition
- Foreign Key: Payment.TuitionId → Tuition.Id
- Cascade delete: When tuition is deleted, all payments are deleted

### Indexes

**Student**
- PRIMARY KEY: Id
- UNIQUE INDEX: StudentNo

**Tuition**
- PRIMARY KEY: Id
- UNIQUE INDEX: (StudentNo, Term)
- INDEX: StudentId

**Payment**
- PRIMARY KEY: Id
- INDEX: TuitionId
- INDEX: StudentId

**RateLimit**
- PRIMARY KEY: Id
- INDEX: (StudentNo, Endpoint, RequestDate)

**User**
- PRIMARY KEY: Id
- UNIQUE INDEX: Username

**ApiLog**
- PRIMARY KEY: Id
- INDEX: RequestTimestamp

### Data Types

**Student**
- Id: INT (Identity)
- StudentNo: VARCHAR(50) NOT NULL
- Name: VARCHAR(200) NOT NULL
- Email: VARCHAR(200)
- CreatedAt: DATETIME NOT NULL

**Tuition**
- Id: INT (Identity)
- StudentId: INT NOT NULL
- StudentNo: VARCHAR(50) NOT NULL
- Term: VARCHAR(50) NOT NULL
- Amount: DECIMAL(18,2) NOT NULL
- Balance: DECIMAL(18,2) NOT NULL
- IsPaid: BIT NOT NULL
- CreatedAt: DATETIME NOT NULL
- PaidAt: DATETIME NULL

**Payment**
- Id: INT (Identity)
- TuitionId: INT NOT NULL
- StudentId: INT NOT NULL
- StudentNo: VARCHAR(50) NOT NULL
- Term: VARCHAR(50) NOT NULL
- Amount: DECIMAL(18,2) NOT NULL
- Status: VARCHAR(50) NOT NULL
- PaymentDate: DATETIME NOT NULL
- ErrorMessage: VARCHAR(MAX) NULL

**RateLimit**
- Id: INT (Identity)
- StudentNo: VARCHAR(50) NOT NULL
- Endpoint: VARCHAR(200) NOT NULL
- RequestDate: DATETIME NOT NULL
- RequestCount: INT NOT NULL

**User**
- Id: INT (Identity)
- Username: VARCHAR(100) NOT NULL
- PasswordHash: VARCHAR(MAX) NOT NULL
- Role: VARCHAR(50) NOT NULL
- CreatedAt: DATETIME NOT NULL

**ApiLog**
- Id: INT (Identity)
- HttpMethod: VARCHAR(10)
- RequestPath: VARCHAR(500)
- RequestTimestamp: DATETIME NOT NULL
- SourceIpAddress: VARCHAR(50)
- RequestHeaders: VARCHAR(MAX)
- RequestSize: BIGINT
- AuthenticationSucceeded: BIT
- StatusCode: INT
- ResponseLatencyMs: BIGINT
- ResponseSize: BIGINT
- ErrorMessage: VARCHAR(MAX)

### Business Rules

1. **Student**
   - StudentNo must be unique
   - Auto-created if not exists when tuition is added

2. **Tuition**
   - Unique per student per term
   - Balance cannot be negative
   - IsPaid is true when Balance = 0
   - Amount is immutable after creation

3. **Payment**
   - Amount must be positive
   - Cannot exceed tuition balance
   - Status values: "Successful", "Partial", "Error"
   - Updates Tuition.Balance on successful payment

4. **RateLimit**
   - Tracks requests per student per endpoint per day
   - Resets daily at midnight UTC
   - Maximum 3 requests for mobile endpoints

5. **User**
   - Passwords are hashed using BCrypt
   - Roles: "Admin", "Banking"
   - Username must be unique

6. **ApiLog**
   - Immutable audit trail
   - Automatically created for all requests
   - Sensitive headers (Authorization) are excluded
