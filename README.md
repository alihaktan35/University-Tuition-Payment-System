# University Tuition Payment System - SE 4458 Midterm Project

**Course**: SE 4458 - Software Architecture & Design of Modern Large Scale Systems

**Student**: Ali Haktan SIĞIN

**Academic Year**: 2025-2026

**Project**: Group 2 - API Project for University Tuition Payment System

---

## 🔗 Live Deployment & Links

- **🌐 Live API Gateway (Azure)**: https://ahs-tuition-gateway.azurewebsites.net/swagger/index.html
- **📹 Video Presentation**: [Will be added - Project demonstration video]
- **💻 GitHub Repository**: https://github.com/alihaktan35/University-Tuition-Payment-System

---

## ☁️ Azure Deployment Architecture

This project is fully deployed on Microsoft Azure with the following components:

| Component | Azure Service | Description |
|-----------|---------------|-------------|
| **API Gateway** | Azure App Service (Web App) | YARP reverse proxy with rate limiting and logging |
| **Main API** | Azure App Service (Web App) | ASP.NET Core Web API with business logic |
| **Database** | Azure SQL Database (SQL Server) | Relational database with EF Core migrations |

**Architecture**:
```
Client → API Gateway (Web App) → Main API (Web App) → Azure SQL Database
```

All services communicate over HTTPS in production. The API Gateway serves as the single entry point for all requests.

---

## 📋 Midterm Requirements Compliance

### ✅ Required API Endpoints (Group 2)

| Endpoint | Parameters | Response | Auth | Paging | Status |
|----------|-----------|----------|------|--------|--------|
| **University Mobile App** |
| Query Tuition | studentNo | tuitionTotal, balance | NO | NO | ✅ Implemented |
| **Banking App** |
| Query Tuition | studentNo | tuitionTotal, balance | YES | NO | ✅ Implemented |
| Pay Tuition | studentNo, term | paymentStatus, remaining | NO | NO | ✅ Implemented |
| **Admin Portal** |
| Add Tuition | studentNo, term | transactionStatus | YES | NO | ✅ Implemented |
| Add Tuition Batch | CSV file | transactionStatus | YES | NO | ✅ Implemented |
| Unpaid Tuition Status | term | list of students | YES | YES | ✅ Implemented |

**Special Features**:
- ✅ **Rate Limiting**: Mobile app endpoint limited to 3 requests per student per day
- ✅ **Partial Payments**: Pay Tuition endpoint supports partial payments with balance tracking
- ✅ **CSV Batch Upload**: Supports bulk tuition import with validation

---

### ✅ Common Requirements Fulfilled

#### 1. API Gateway Implementation
- ✅ **Technology**: YARP (Yet Another Reverse Proxy)
- ✅ **Single Entry Point**: All requests go through Gateway
- ✅ **Rate Limiting**: Implemented at Gateway level (database-backed)
- ✅ **Comprehensive Logging**: All required fields captured
  - Request: HTTP method, full path, timestamp, source IP, headers, size
  - Response: Status code, latency (ms), size
  - Authentication: Success/failure status
  - Mapping template failures: Logged

#### 2. Authentication
- ✅ **JWT (JSON Web Tokens)** with role-based authorization
- ✅ Roles: Admin, BankingSystem
- ✅ Login endpoint: `POST /api/v1/auth/login`

#### 3. Versioning
- ✅ All REST services use `/api/v1/` versioning

#### 4. Swagger Documentation
- ✅ Swagger UI configured and pointing to API Gateway URL
- ✅ All endpoints documented with examples

#### 5. Cloud Deployment
- ✅ Deployed to Azure App Service
- ✅ Azure SQL Database configured
- ✅ Production environment active

#### 6. Database Model
- ✅ Complete ER diagram provided below
- ✅ Entity Framework Core migrations
- ✅ Proper relationships and foreign keys

---

## 🏗️ System Architecture

```
┌─────────────────┐
│  Mobile App     │
│  (Rate Limited) │
└────────┬────────┘
         │
┌────────▼────────┐     ┌──────────────────────┐
│  Banking App    │────▶│   API Gateway        │
│ (Authenticated) │     │  (Azure Web App)     │
└─────────────────┘     │  - YARP Proxy        │
                        │  - Rate Limiting     │
┌─────────────────┐     │  - Logging           │
│   Admin Portal  │────▶└──────────┬───────────┘
│ (Authenticated) │                │
└─────────────────┘                │
                        ┌──────────▼───────────┐
                        │  Main API            │
                        │  (Azure Web App)     │
                        │  - Controllers       │
                        │  - JWT Auth          │
                        │  - Business Logic    │
                        └──────────┬───────────┘
                                   │
                        ┌──────────▼───────────┐
                        │  Azure SQL Database  │
                        │  - Students          │
                        │  - Tuitions          │
                        │  - Payments          │
                        │  - Users             │
                        └──────────────────────┘
```

---

## 🗄️ Data Model (ER Diagram)

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
         ┌────────────────┴───-──┐
         │        TUITION        │
         ├─────────────────────-─┤
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
- RATE_LIMIT: Tracks API Gateway rate limiting (student_no, endpoint, date, call_count)
- USER: Stores admin and banking system users with hashed passwords
```

---

## 📡 API Endpoints Overview

### 1. Authentication
**POST** `/api/v1/auth/login` - Get JWT token

### 2. Mobile App (No Authentication Required)
**GET** `/api/v1/tuition/query/{studentNo}` - Query tuition (Rate limited: 3/day)

### 3. Banking App
**GET** `/api/v1/banking/tuition/{studentNo}` - Query tuition (Requires JWT)
**POST** `/api/v1/banking/pay` - Process payment (No authentication)

### 4. Admin Portal (Requires Admin Role)
**POST** `/api/v1/admin/tuition` - Add single tuition
**POST** `/api/v1/admin/tuition/batch` - Batch upload via CSV
**GET** `/api/v1/admin/unpaid/{term}?page=1&pageSize=20` - Get unpaid list (Paginated)

---

## 🛠️ Technology Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core 9.0 |
| Language | C# 12.0 |
| Database | Azure SQL Database |
| ORM | Entity Framework Core 9.0 |
| API Gateway | YARP 2.3.0 |
| Authentication | JWT |
| Password Hashing | BCrypt.Net |
| CSV Processing | CsvHelper |
| API Documentation | Swagger UI (Swashbuckle) |
| Hosting | Azure App Service |

---

## 🧪 Test Credentials

### Test Users (Pre-seeded in Azure SQL)
| Username | Password | Role |
|----------|----------|------|
| admin | Admin123! | Admin |
| bankapi | Bank123! | BankingSystem |

### Test Students (Pre-seeded in Azure SQL)
| StudentNo | Name | Term | Tuition | Balance | Status |
|-----------|------|------|---------|---------|--------|
| 20210001 | Ahmet Yılmaz | 2024-Fall | 50,000 TRY | 50,000 TRY | UNPAID |
| 20210002 | Ayşe Demir | 2024-Fall | 50,000 TRY | 25,000 TRY | PARTIAL |
| 20210003 | Mehmet Kaya | 2024-Fall | 50,000 TRY | 0 TRY | PAID |

---

## 📋 Design Assumptions

### Business Rules
1. **Term Format**: "YYYY-Season" (e.g., "2024-Fall", "2025-Spring")
2. **Currency**: Turkish Lira (TRY)
3. **Student Numbers**: Alphanumeric, 6-10 characters
4. **JWT Expiration**: 24 hours
5. **Rate Limit Reset**: Midnight UTC daily
6. **Partial Payments**: Supported - remaining balance is tracked
7. **CSV Format**: UTF-8 encoding, headers: studentNo, term, amount
8. **Pagination**: Default 20 records per page, maximum 100

### Technical Assumptions
1. **Database**: SQLite for local development, Azure SQL for production
2. **Authentication**: JWT only (no refresh tokens)
3. **Rate Limiting**: Per student/endpoint/date, database-backed for distributed systems
4. **Payment Processing**: No actual banking integration (simulated)
5. **Transaction IDs**: Generated using GUID
6. **Timestamps**: All in UTC
7. **HTTPS**: Required in production environment
8. **CORS**: Configured for cross-origin requests

---

## 🔑 Key Features Demonstrating Requirements

### ✅ All Required Endpoints
- 6 endpoints implemented exactly as specified in requirements
- Correct authentication and paging on specified endpoints
- Rate limiting on mobile endpoint (3 requests/day)

### ✅ API Gateway with YARP
- Single entry point for all API requests
- Database-backed rate limiting at gateway level
- Comprehensive request/response logging
- Proper routing to backend API

### ✅ Authentication & Authorization
- JWT-based authentication
- Role-based authorization (Admin, BankingSystem)
- Secure password hashing with BCrypt

### ✅ Database Design
- Proper normalized schema
- Foreign key relationships
- EF Core migrations
- Seed data for testing

### ✅ Cloud Deployment
- Deployed to Azure App Service
- Azure SQL Database
- Production environment configured
- HTTPS enabled

### ✅ Documentation
- Swagger UI integrated
- Complete README with ER diagram
- Design assumptions documented
- Issues and solutions documented

---

## 📁 Project Structure

```
University-Tuition-Payment-System/
├── src/
│   ├── APIGateway/                    # API Gateway (YARP)
│   │   ├── Middleware/
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Data/GatewayDbContext.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── TuitionPaymentAPI/             # Main API
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── TuitionController.cs
│       │   ├── BankingController.cs
│       │   └── AdminController.cs
│       ├── Data/TuitionDbContext.cs
│       ├── DTOs/
│       ├── Models/
│       ├── Services/
│       └── Migrations/
│
├── README.md
└── UniversityTuitionSystem.sln
```

---

## 🚀 How to Test the Live Deployment

### 1. Access Swagger UI
Visit: https://ahs-tuition-gateway.azurewebsites.net/swagger/index.html

### 2. Login to Get JWT Token
```bash
POST /api/v1/auth/login
{
  "username": "admin",
  "password": "Admin123!"
}
```
Copy the returned token.

### 3. Test Mobile Endpoint (No Auth, Rate Limited)
```bash
GET /api/v1/tuition/query/20210001
```
Try calling 4 times - the 4th call should return 429 (Rate Limit Exceeded).

### 4. Test Banking Endpoint (With Auth)
Click "Authorize" in Swagger UI, enter: `Bearer YOUR_TOKEN`
```bash
GET /api/v1/banking/tuition/20210001
```

### 5. Test Payment Processing
```bash
POST /api/v1/banking/pay
{
  "studentNo": "20210001",
  "term": "2024-Fall",
  "amount": 10000
}
```

### 6. Test Admin Endpoints (Requires Admin Token)
```bash
GET /api/v1/admin/unpaid/2024-Fall?page=1&pageSize=10
```

### 7. Test CSV Batch Upload
Upload a CSV file with headers: `studentNo,term,amount`

---

## 📚 Source Code

- **GitHub Repository**: https://github.com/alihaktan35/University-Tuition-Payment-System
- **Live Deployment**: https://ahs-tuition-gateway.azurewebsites.net/swagger/index.html

---

## 🎓 Academic Information

**Course**: SE 4458 - Software Architecture & Design of Modern Large Scale Systems

**Project Type**: Midterm Project - Group 2 (University Tuition Payment System)

**Student**: Ali Haktan SIĞIN (@alihaktan35)

**Academic Year**: 2025-2026

**Semester**: Fall 2025

**Submission Date**: November 2025

---

## ✅ Requirements Checklist

- ✅ All 6 required API endpoints implemented
- ✅ API Gateway with YARP reverse proxy
- ✅ Rate limiting at Gateway level (3 requests/day for mobile)
- ✅ Comprehensive Gateway logging (request/response details)
- ✅ JWT authentication with role-based authorization
- ✅ API versioning (v1)
- ✅ Paging support on Unpaid Tuition endpoint
- ✅ CSV batch upload functionality
- ✅ Swagger UI documentation
- ✅ Database model (ER diagram)
- ✅ Azure SQL Database deployment
- ✅ Azure App Service deployment (2 Web Apps)
- ✅ README with design, assumptions, and issues
- ✅ GitHub repository
- ⏳ Video presentation (to be added)

---

**Built with ASP.NET Core 9.0 | Deployed on Microsoft Azure | Documented with Swagger UI**
