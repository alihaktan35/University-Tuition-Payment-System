# University Tuition Payment System API

A comprehensive REST API system for managing university tuition payments with an API Gateway, rate limiting, JWT authentication, and full CRUD operations.

**Course**: SE 4458 - Büyük Ölçekli Sistemler İçin Sistem Mimarisi

**Project Type**: Midterm Project

**Academic Year**: 2025-2026

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Links](#links)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Features](#features)
- [API Endpoints](#api-endpoints)
- [Data Model](#data-model)
- [Getting Started](#getting-started)
- [Testing](#testing)
- [Deployment](#deployment)
- [Assumptions](#assumptions)
- [Issues & Solutions](#issues--solutions)

---

## 🎯 Project Overview

This project implements a REST API system for managing university tuition payments. The system consists of:

1. **Main API** (`TuitionPaymentAPI`) - Handles all business logic, authentication, and database operations
2. **API Gateway** (`APIGateway`) - Routes all requests, provides logging, and acts as single entry point

The system supports three types of clients:
- **University Mobile App**: Students query their tuition (rate-limited to 3 requests/day)
- **Banking App**: Banks query tuition and process payments (authenticated)
- **Admin Portal**: Administrators manage tuition records (authenticated)

---

## 🔗 Links

- **GitHub Repository**: [University-Tuition-Payment-System](https://github.com/alihaktan35/University-Tuition-Payment-System)
- **Local API Documentation (Swagger UI)**: http://localhost:5000/swagger (when running locally)
- **Deployed API**: `Coming soon - Will be deployed to Azure`

---

## 🏗️ Architecture

### System Architecture Diagram

```
┌─────────────────┐
│  Mobile App     │
│  (Rate Limited) │
└────────┬────────┘
         │
┌────────▼────────┐     ┌──────────────────┐
│  Banking App    │────▶│   API Gateway    │
│ (Authenticated) │     │  (Port 5000)     │
└─────────────────┘     │  - Logging       │
                        │  - Routing       │
┌─────────────────┐     │  - YARP Proxy    │
│   Admin Portal  │────▶└────────┬─────────┘
│ (Authenticated) │              │
└─────────────────┘              │
                        ┌────────▼─────────┐
                        │  Tuition API     │
                        │  (Port 5001)     │
                        │  - Controllers   │
                        │  - Auth (JWT)    │
                        │  - Rate Limiting │
                        │  - Middleware    │
                        └────────┬─────────┘
                                 │
                        ┌────────▼─────────┐
                        │   SQLite DB      │
                        │  (Local Dev)     │
                        │  or Azure SQL    │
                        └──────────────────┘
```

### Request Flow

1. Client sends request to **API Gateway** (localhost:5000)
2. Gateway logs request details (method, path, IP, headers, timestamp)
3. Gateway forwards request to **Main API** (localhost:5001)
4. Main API processes request:
   - Rate limiting (for mobile endpoints)
   - Authentication validation (JWT)
   - Business logic execution
   - Database operations
5. Main API returns response
6. Gateway logs response (status, latency, size)
7. Gateway returns response to client

---

## 🛠️ Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | ASP.NET Core Web API | 9.0 |
| **Language** | C# | 12.0 |
| **Database (Dev)** | SQLite | 9.0 |
| **Database (Prod)** | Azure SQL Database | - |
| **ORM** | Entity Framework Core | 9.0 |
| **API Gateway** | YARP (Yet Another Reverse Proxy) | 2.3.0 |
| **Authentication** | JWT (JSON Web Tokens) | - |
| **Password Hashing** | BCrypt.Net-Next | Latest |
| **CSV Processing** | CsvHelper | Latest |
| **API Documentation** | Swagger UI (Swashbuckle) | 7.2.0 |

---

## ✨ Features

### Core Functionality
- ✅ Student tuition query (mobile & banking apps)
- ✅ Payment processing with partial payment support
- ✅ Admin tuition management (single & batch CSV upload)
- ✅ Unpaid tuition reporting with pagination
- ✅ JWT-based authentication
- ✅ Role-based authorization (Admin, BankingSystem)

### Technical Features
- ✅ API Gateway with YARP reverse proxy
- ✅ Comprehensive request/response logging
- ✅ Rate limiting (3 requests/day for mobile endpoint)
- ✅ Automatic database migrations and seeding
- ✅ Swagger UI for API testing and documentation
- ✅ CORS enabled
- ✅ Error handling with standardized responses

---

## 📡 API Endpoints

### Authentication

#### POST `/api/v1/auth/login`
Authenticate and get JWT token

**Request**:
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 86400,
  "username": "admin",
  "role": "Admin"
}
```

**Example Usage**:
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

---

### Mobile App Endpoints

#### GET `/api/v1/tuition/query/{studentNo}`
Query tuition for a student
- **Authentication**: None
- **Rate Limit**: 3 requests per student per day

**Example**: `GET /api/v1/tuition/query/20210001`

**Response**:
```json
{
  "tuitionTotal": 50000.00,
  "balance": 50000.00
}
```

**Example Usage**:
```bash
# Query student 20210001's tuition
curl http://localhost:5000/api/v1/tuition/query/20210001

# Query student 20210002's tuition
curl http://localhost:5000/api/v1/tuition/query/20210002
```

---

### Banking App Endpoints

#### GET `/api/v1/banking/tuition/{studentNo}`
Query tuition (requires authentication)
- **Authentication**: Required (JWT)

**Example**: `GET /api/v1/banking/tuition/20210001`
**Headers**: `Authorization: Bearer {token}`

**Response**:
```json
{
  "tuitionTotal": 50000.00,
  "balance": 50000.00
}
```

**Example Usage**:
```bash
# Replace YOUR_TOKEN with actual JWT token from login
curl http://localhost:5000/api/v1/banking/tuition/20210001 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### POST `/api/v1/banking/pay`
Process tuition payment
- **Authentication**: None

**Request**:
```json
{
  "studentNo": "20210001",
  "term": "2024-Fall",
  "amount": 10000.00
}
```

**Response**:
```json
{
  "status": "Successful",
  "remainingBalance": 40000.00,
  "message": "Partial payment of 10000.00 processed successfully for term 2024-Fall"
}
```

**Example Usage**:
```bash
# Partial payment
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2024-Fall","amount":10000.00}'

# Full payment
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210004","term":"2024-Fall","amount":50000.00}'
```

---

### Admin Endpoints (All require Admin role authentication)

#### POST `/api/v1/admin/tuition`
Add tuition for single student
- **Authentication**: Required (Admin role)

**Request**:
```json
{
  "studentNo": "20210001",
  "term": "2025-Spring",
  "amount": 55000.00
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tuition added successfully for student 20210001, term 2025-Spring"
}
```

**Example Usage**:
```bash
# Replace YOUR_ADMIN_TOKEN with admin JWT token
curl -X POST http://localhost:5000/api/v1/admin/tuition \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2025-Spring","amount":55000.00}'
```

#### POST `/api/v1/admin/tuition/batch`
Batch upload tuition via CSV
- **Authentication**: Required (Admin role)
- **Content-Type**: `multipart/form-data`
- **CSV Format**: `studentNo,term,amount`

**Response**:
```json
{
  "successCount": 5,
  "errorCount": 0,
  "errors": []
}
```

**Example Usage**:
```bash
# First create a CSV file
cat > tuition_batch.csv << 'EOF'
studentNo,term,amount
20210001,2025-Fall,60000
20210002,2025-Fall,60000
20210003,2025-Fall,60000
20210004,2025-Fall,60000
20210005,2025-Fall,60000
EOF

# Upload the CSV file
curl -X POST http://localhost:5000/api/v1/admin/tuition/batch \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -F "file=@tuition_batch.csv"
```

#### GET `/api/v1/admin/unpaid/{term}?page=1&pageSize=20`
Get unpaid tuition list with pagination
- **Authentication**: Required (Admin role)

**Response**:
```json
{
  "data": [
    {
      "studentNo": "20210001",
      "studentName": "Ahmet Yılmaz",
      "term": "2024-Fall",
      "totalAmount": 50000.00,
      "balance": 50000.00,
      "status": "UNPAID"
    },
    {
      "studentNo": "20210002",
      "studentName": "Ayşe Demir",
      "term": "2024-Fall",
      "totalAmount": 50000.00,
      "balance": 25000.00,
      "status": "PARTIAL"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 2,
    "totalPages": 1
  }
}
```

**Example Usage**:
```bash
# Get unpaid tuition for 2024-Fall term
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"

# Get with custom pagination
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

---

## 🗄️ Data Model

### ER Diagram

```
┌─────────────────┐
│    STUDENT      │
├─────────────────┤
│ student_id (PK) │
│ student_no (UQ) │◄──────┐
│ name            │       │
│ email           │       │
│ created_at      │       │
└─────────────────┘       │
                          │
         ┌────────────────┴─────┐
         │       TUITION         │
         ├──────────────────────┤
         │ tuition_id (PK)       │
         │ student_id (FK)       │──────┐
         │ term                  │      │
         │ total_amount          │      │
         │ balance               │      │
         │ paid_amount           │      │
         │ status                │      │
         │ created_at/updated_at │      │
         └───────────────────────┘      │
                                        │
                       ┌────────────────▼────┐
                       │     PAYMENT         │
                       ├─────────────────────┤
                       │ payment_id (PK)     │
                       │ tuition_id (FK)     │
                       │ amount              │
                       │ payment_date        │
                       │ status              │
                       │ transaction_ref     │
                       └─────────────────────┘

Additional Tables:
- RATE_LIMIT: Tracks API rate limiting
- USER: Stores admin and banking system users
```

---

## 🚀 Getting Started

### Prerequisites
- **.NET SDK 9.0** or higher
- **Git**

### Installation

1. **Clone the repository**:
```bash
git clone https://github.com/yourusername/University-Tuition-Payment-System.git
cd University-Tuition-Payment-System
```

2. **Restore packages**:
```bash
dotnet restore
```

3. **Build the solution**:
```bash
dotnet build
```

### Running Locally

Run both applications in separate terminals:

**Terminal 1 - Main API (Port 5001)**:
```bash
cd src/TuitionPaymentAPI
dotnet run
```

**Terminal 2 - API Gateway (Port 5000)**:
```bash
cd src/APIGateway
dotnet run
```

### Access Points

- **API Gateway (Main Entry)**: http://localhost:5000
- **API Documentation (Swagger UI)**: http://localhost:5000/swagger
- **Direct API Access**: http://localhost:5001 (for testing only)

---

## 🧪 Testing

### Test Users

| Username | Password | Role |
|----------|----------|------|
| admin | Admin123! | Admin |
| bankapi | Bank123! | BankingSystem |

### Test Students (Pre-seeded)

| StudentNo | Name | Tuition | Balance | Status |
|-----------|------|---------|---------|--------|
| 20210001 | Ahmet Yılmaz | 50,000 | 50,000 | UNPAID |
| 20210002 | Ayşe Demir | 50,000 | 25,000 | PARTIAL |
| 20210003 | Mehmet Kaya | 50,000 | 0 | PAID |

### Quick Test Commands

1. **Login**:
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

2. **Query Tuition (No Auth)**:
```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
```

3. **Process Payment**:
```bash
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2024-Fall","amount":10000}'
```

4. **Test Rate Limiting** (call 4 times, 4th should return 429):
```bash
for i in {1..4}; do
  curl http://localhost:5000/api/v1/tuition/query/20210001
  echo ""
done
```

---

## 🚢 Deployment

### Azure Deployment Steps

1. **Create Azure SQL Database**
2. **Update connection string** in appsettings.json
3. **Change to UseSqlServer** in Program.cs
4. **Deploy Main API** to Azure App Service
5. **Deploy API Gateway** (update backend URL in appsettings.json)
6. **Run migrations** against Azure SQL

### Environment Variables (Azure App Service)

```
JwtSettings__SecretKey=<your-production-secret>
ConnectionStrings__DefaultConnection=<azure-sql-connection-string>
```

---

## 📋 Assumptions

### Business Logic
1. Term format: "YYYY-Season" (e.g., "2024-Fall")
2. Currency: Turkish Lira (TRY)
3. Student numbers: Alphanumeric, 6-10 characters
4. JWT expiration: 24 hours
5. Rate limit reset: Midnight UTC
6. Minimum payment: 0.01 TRY
7. CSV encoding: UTF-8
8. Max CSV size: 10 MB
9. Default page size: 20 records
10. Max page size: 100 records

### Technical
1. SQLite for local dev, Azure SQL for production
2. No refresh tokens (JWT only)
3. Rate limiting per student/endpoint/date
4. No actual banking integration
5. Transaction IDs generated with GUID
6. API versioning: v1
7. CORS: Open (restrict in production)
8. Timestamps: UTC
9. HTTPS required in production
10. Console logging (use Azure App Insights in prod)

---

## 🐛 Issues & Solutions

### Issue 1: macOS Doesn't Support SQL Server LocalDB
**Solution**: Used SQLite for local development. Production will use Azure SQL. Entity Framework Core makes switching seamless.

### Issue 2: Microsoft.OpenApi Namespace Issues
**Solution**: Added explicit Microsoft.OpenApi package version 2.3.0 and simplified Swagger configuration.

### Issue 3: Duplicate ErrorDetail Classes
**Solution**: Renamed BatchUploadResponse's ErrorDetail to BatchErrorDetail to avoid namespace collision.

### Issue 4: Rate Limiting Implementation
**Solution**: Created custom middleware with database-backed rate limit tracking using composite index (student_no, endpoint, date).

### Issue 5: API Gateway Logging Requirements
**Solution**: Implemented custom middleware in Gateway that logs all request/response details including headers, status codes, latency, and body sizes.

### Issue 6: Entity Framework Model Changes with DateTime.UtcNow
**Solution**: Replaced `DateTime.UtcNow` in seed data with fixed DateTime values. Moved User seeding from DbContext to Program.cs to handle BCrypt's non-deterministic hashing.

---

## 📁 Project Structure

```
University-Tuition-Payment-System/
├── src/
│   ├── TuitionPaymentAPI/          # Main API
│   │   ├── Controllers/
│   │   ├── Data/
│   │   ├── DTOs/
│   │   ├── Middleware/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Migrations/
│   │
│   └── APIGateway/                 # Gateway
│       ├── Program.cs
│       └── appsettings.json
│
├── README.md
└── UniversityTuitionSystem.sln
```

---

## 🔑 Key Features Implemented

- ✅ All 6 required API endpoints
- ✅ JWT authentication with role-based authorization
- ✅ Rate limiting (3 requests/day for mobile)
- ✅ Partial payment support
- ✅ CSV batch upload with validation
- ✅ Pagination for unpaid list
- ✅ API Gateway with YARP
- ✅ Comprehensive logging
- ✅ Swagger UI API documentation
- ✅ Database seeding with test data
- ✅ Error handling with standard responses
- ✅ Entity Framework migrations

---

## 📚 References

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [JWT.IO](https://jwt.io/)
- [Swagger/OpenAPI](https://swagger.io/)

---

## 🎓 Academic Information

**Course**: SE 4458 - Büyük Ölçekli Sistemler İçin Sistem Mimarisi

**Project**: Midterm Project

**Semester**: Fall 2025

**Student**: Ali Haktan SIĞIN (@alihaktan35)

---

**Built with ASP.NET Core 9.0 | Documented with Swagger UI | Ready for Azure**
