# University Tuition Payment System API

## Project Information
- **Course**: SE 4458 Software Architecture & Design of Modern Large Scale Systems
- **Assignment**: Midterm Project - Group 2
- **Project Type**: University Tuition Payment System API

## Source Code
- **Repository**: [Add your GitHub repository link here]
- **Technology Stack**:
  - .NET 8.0 / ASP.NET Core
  - Entity Framework Core
  - SQL Server / Azure SQL Database
  - JWT Authentication
  - Swagger/OpenAPI

## Project Overview

This project implements a RESTful API system for managing university tuition payments. The system serves three main client types:

1. **University Mobile App** - Students can query their tuition status
2. **Banking App** - Banks can query tuition and process payments
3. **University Admin Portal** - Administrators can manage tuition records

### Key Features
- RESTful API with versioning (v1)
- JWT-based authentication for secure endpoints
- Rate limiting (3 requests per day for mobile queries)
- Comprehensive request/response logging
- Paging support for large datasets
- Batch CSV upload for tuition records
- Partial payment handling
- Azure-ready deployment configuration

## Architecture & Design

### Design Decisions

1. **Layered Architecture**
   - Controllers: Handle HTTP requests/responses
   - Services: Business logic layer
   - Data: Database context and models
   - DTOs: Data transfer objects for API contracts

2. **Database Design**
   - Normalized relational database design
   - Foreign key relationships for data integrity
   - Indexes on frequently queried fields (StudentNo, Term)

3. **Authentication Strategy**
   - JWT tokens for Banking and Admin APIs
   - No authentication for Mobile app (rate-limited instead)
   - No authentication for payment endpoint (public-facing)

4. **Rate Limiting Strategy**
   - Database-backed rate limiting
   - Per-student, per-endpoint tracking
   - Daily reset mechanism

### Assumptions

1. **Student Management**
   - Students are auto-created when tuition is added if they don't exist
   - StudentNo is the primary identifier (unique)

2. **Payment Processing**
   - No actual payment gateway integration (as per requirements)
   - Partial payments are supported and tracked
   - Balance is automatically updated after payment

3. **Term Format**
   - Terms follow format: "YYYY-Season" (e.g., "2024-Fall", "2025-Spring")

4. **Security**
   - JWT tokens expire after 24 hours
   - Passwords are hashed using BCrypt
   - Sensitive headers are not logged

5. **Deployment**
   - Designed for Azure App Service
   - Azure SQL Database for production
   - LocalDB for development

## Data Model

### Entity Relationship Diagram (Description)

```
┌─────────────────┐
│    Student      │
├─────────────────┤
│ Id (PK)         │
│ StudentNo (UK)  │──┐
│ Name            │  │
│ Email           │  │
│ CreatedAt       │  │
└─────────────────┘  │
                     │
                     │ 1:N
                     │
┌─────────────────┐  │    ┌─────────────────┐
│    Tuition      │◄─┘    │    Payment      │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │──────►│ Id (PK)         │
│ StudentId (FK)  │  1:N  │ TuitionId (FK)  │
│ StudentNo       │       │ StudentId (FK)  │
│ Term            │       │ StudentNo       │
│ Amount          │       │ Term            │
│ Balance         │       │ Amount          │
│ IsPaid          │       │ Status          │
│ CreatedAt       │       │ PaymentDate     │
│ PaidAt          │       │ ErrorMessage    │
└─────────────────┘       └─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│   RateLimit     │       │      User       │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ StudentNo       │       │ Username (UK)   │
│ Endpoint        │       │ PasswordHash    │
│ RequestDate     │       │ Role            │
│ RequestCount    │       │ CreatedAt       │
└─────────────────┘       └─────────────────┘

┌─────────────────┐
│     ApiLog      │
├─────────────────┤
│ Id (PK)         │
│ HttpMethod      │
│ RequestPath     │
│ RequestTimestamp│
│ SourceIpAddress │
│ RequestHeaders  │
│ RequestSize     │
│ AuthSucceeded   │
│ StatusCode      │
│ ResponseLatency │
│ ResponseSize    │
│ ErrorMessage    │
└─────────────────┘
```

### Key Entities

**Student**
- Primary entity for student information
- Unique StudentNo as business key

**Tuition**
- Tracks tuition per student per term
- Unique constraint on (StudentNo, Term)
- Maintains balance for partial payments

**Payment**
- Records all payment transactions
- Links to both Student and Tuition
- Tracks payment status (Successful, Partial, Error)

**RateLimit**
- Tracks API call frequency
- Enforces 3 calls per student per day limit

**User**
- Authentication for Banking and Admin users
- Role-based access control

**ApiLog**
- Comprehensive logging of all API requests
- Tracks performance, authentication, and errors

## API Endpoints

### Base URL
- Development: `https://localhost:5001`
- Production: `https://[your-app-name].azurewebsites.net`

### API Versioning
All endpoints are versioned: `/api/v1/...`

---

### Authentication Endpoints

#### POST /api/v1/auth/login
Authenticate and receive JWT token

**Request Body:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "role": "Admin"
}
```

**Default Credentials:**
- Admin: `username: admin, password: admin123`
- Banking: `username: banking, password: banking123`

---

### Mobile App Endpoints

#### GET /api/v1/mobile/tuition/{studentNo}
Query tuition information for a student

**Authentication:** None
**Rate Limit:** 3 requests per student per day
**Paging:** No

**Response:**
```json
{
  "studentNo": "2021001",
  "tuitionTotal": 30000.00,
  "balance": 15000.00,
  "isPaid": false
}
```

**Status Codes:**
- 200: Success
- 404: Student not found
- 429: Rate limit exceeded

---

### Banking App Endpoints

#### GET /api/v1/banking/tuition/{studentNo}
Query tuition information (authenticated)

**Authentication:** Required (Banking or Admin role)
**Rate Limit:** None
**Paging:** No

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** Same as mobile endpoint

**Status Codes:**
- 200: Success
- 401: Unauthorized
- 404: Student not found

---

#### POST /api/v1/banking/payment
Process tuition payment

**Authentication:** None
**Rate Limit:** None
**Paging:** No

**Request Body:**
```json
{
  "studentNo": "2021001",
  "term": "2024-Fall",
  "amount": 7500.00
}
```

**Response:**
```json
{
  "status": "Partial",
  "amountPaid": 7500.00,
  "remainingBalance": 7500.00,
  "message": "Partial payment processed. Remaining balance: $7,500.00"
}
```

**Status Values:**
- `Successful`: Full payment completed
- `Partial`: Partial payment, balance remains
- `Error`: Payment failed

**Status Codes:**
- 200: Success
- 400: Bad request (invalid amount, already paid, etc.)

---

### Admin Endpoints

All admin endpoints require authentication with Admin role.

#### POST /api/v1/admin/tuition
Add tuition for a single student

**Authentication:** Required (Admin role)
**Rate Limit:** None
**Paging:** No

**Request Body:**
```json
{
  "studentNo": "2021009",
  "term": "2024-Fall",
  "amount": 15000.00
}
```

**Response:**
```json
{
  "status": "Success",
  "message": "Tuition added successfully for student 2021009",
  "recordsProcessed": 1
}
```

**Status Codes:**
- 200: Success
- 400: Error (duplicate, invalid data)
- 401: Unauthorized

---

#### POST /api/v1/admin/tuition/batch
Batch upload tuitions from CSV file

**Authentication:** Required (Admin role)
**Rate Limit:** None
**Paging:** No

**Request:** multipart/form-data with CSV file

**CSV Format:**
```csv
StudentNo,Term,Amount
2021006,2024-Fall,15000.00
2021007,2025-Spring,16000.00
```

**Response:**
```json
{
  "status": "Success",
  "message": "Batch processing complete. 2 records added successfully.",
  "recordsProcessed": 2
}
```

**Status Codes:**
- 200: Success
- 400: Invalid file format
- 401: Unauthorized

---

#### GET /api/v1/admin/tuition/unpaid/{term}
Get list of students with unpaid tuition

**Authentication:** Required (Admin role)
**Rate Limit:** None
**Paging:** Yes

**Query Parameters:**
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Records per page (default: 10, max: 100)

**Example:**
```
GET /api/v1/admin/tuition/unpaid/2024-Fall?pageNumber=1&pageSize=10
```

**Response:**
```json
{
  "term": "2024-Fall",
  "students": [
    {
      "studentNo": "2021002",
      "name": "Ayşe Kaya",
      "tuitionAmount": 15000.00,
      "balance": 15000.00
    }
  ],
  "totalCount": 4,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**Status Codes:**
- 200: Success
- 401: Unauthorized

---

## Logging

The API implements comprehensive logging as per requirements:

### Request-Level Logs
- HTTP method (GET/POST/PUT/DELETE)
- Full request path (e.g., `/api/v1/tuition/2021001`)
- Request timestamp
- Source IP address
- Headers received (sensitive headers excluded)
- Request size (bytes)
- Authentication success/failure

### Response-Level Logs
- Status code (200, 400, 401, 403, 500, etc.)
- Response latency (milliseconds)
- Response size (bytes)
- Error messages (if any)

### Log Storage
- Database: `ApiLogs` table
- Console/File: Standard ASP.NET Core logging

---

## Rate Limiting

### Mobile App Endpoint
- **Endpoint:** `/api/v1/mobile/tuition/{studentNo}`
- **Limit:** 3 requests per student per day
- **Reset:** Daily at midnight UTC
- **Response:** HTTP 429 (Too Many Requests) when exceeded

### Implementation
- Database-backed tracking
- Per-student, per-endpoint counters
- Automatic daily reset

---

## Deployment to Azure

### Prerequisites
1. Azure account with active subscription
2. Azure CLI installed
3. .NET 8.0 SDK

### Quick Deployment Steps

1. **Create Azure Resources:**
```bash
# Resource Group
az group create --name UniversityTuitionRG --location eastus

# SQL Database
az sql server create --name university-tuition-sql --resource-group UniversityTuitionRG --admin-user sqladmin --admin-password <password>
az sql db create --resource-group UniversityTuitionRG --server university-tuition-sql --name UniversityTuitionDB --service-objective F1

# App Service
az appservice plan create --name UniversityTuitionPlan --resource-group UniversityTuitionRG --sku F1 --is-linux
az webapp create --name university-tuition-api --resource-group UniversityTuitionRG --plan UniversityTuitionPlan --runtime "DOTNET|8.0"
```

2. **Configure Connection String:**
```bash
az webapp config connection-string set \
  --name university-tuition-api \
  --resource-group UniversityTuitionRG \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="<your-connection-string>"
```

3. **Deploy Application:**
```bash
dotnet publish -c Release -o ./publish
cd publish && zip -r ../deploy.zip . && cd ..
az webapp deployment source config-zip --resource-group UniversityTuitionRG --name university-tuition-api --src deploy.zip
```

**Detailed Instructions:** See [azure-deploy.md](azure-deploy.md)

---

## Testing the API

### Using Swagger UI
1. Navigate to the root URL of your deployed API
2. Swagger UI will load automatically
3. Use the "Authorize" button to add JWT token for protected endpoints

### Sample Test Flow

1. **Login as Admin:**
```bash
curl -X POST https://your-api.azurewebsites.net/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

2. **Query Tuition (Mobile):**
```bash
curl https://your-api.azurewebsites.net/api/v1/mobile/tuition/2021001
```

3. **Add Tuition (Admin):**
```bash
curl -X POST https://your-api.azurewebsites.net/api/v1/admin/tuition \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"studentNo":"2021010","term":"2024-Fall","amount":15000}'
```

4. **Process Payment:**
```bash
curl -X POST https://your-api.azurewebsites.net/api/v1/banking/payment \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"2021001","term":"2024-Fall","amount":5000}'
```

5. **Query Unpaid Tuitions (Admin):**
```bash
curl https://your-api.azurewebsites.net/api/v1/admin/tuition/unpaid/2024-Fall?pageNumber=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
```

### Sample Data

The database is seeded with sample data:

**Students:**
- 2021001 - Ahmet Yılmaz
- 2021002 - Ayşe Kaya
- 2021003 - Mehmet Demir
- 2021004 - Fatma Şahin
- 2021005 - Ali Çelik

**Tuitions:**
- Each student has tuition for 2024-Fall and 2025-Spring
- Amount: 15,000.00 per term
- Some tuitions are paid/partially paid for testing

**Users:**
- Admin: username=`admin`, password=`admin123`
- Banking: username=`banking`, password=`banking123`

---

## Issues Encountered & Solutions

### Issue 1: Rate Limiting Implementation
**Problem:** Deciding between in-memory vs database-backed rate limiting
**Solution:** Chose database-backed for scalability across multiple instances in cloud deployment

### Issue 2: Partial Payment Handling
**Problem:** How to track multiple partial payments for a single tuition
**Solution:** Created separate Payment entity to track all transactions, update Tuition.Balance after each payment

### Issue 3: Authentication Strategy
**Problem:** Different auth requirements for different endpoints
**Solution:** Used role-based authorization, selectively applied [Authorize] attribute per requirements

### Issue 4: CSV Batch Upload
**Problem:** Handling errors in batch processing
**Solution:** Process records individually, continue on error, report summary with success/error counts

### Issue 5: Logging Middleware
**Problem:** Capturing response body without affecting stream
**Solution:** Used MemoryStream to buffer response, then copy back to original stream

---

## Project Structure

```
UniversityTuitionAPI/
├── Controllers/
│   ├── AuthController.cs
│   ├── MobileAppController.cs
│   ├── BankingController.cs
│   └── AdminController.cs
├── Services/
│   ├── IAuthService.cs & AuthService.cs
│   ├── ITuitionService.cs & TuitionService.cs
│   ├── IPaymentService.cs & PaymentService.cs
│   ├── IAdminService.cs & AdminService.cs
│   └── IRateLimitService.cs & RateLimitService.cs
├── Models/
│   ├── Student.cs
│   ├── Tuition.cs
│   ├── Payment.cs
│   ├── RateLimit.cs
│   ├── User.cs
│   └── ApiLog.cs
├── DTOs/
│   ├── TuitionQueryResponse.cs
│   ├── PaymentRequest.cs & PaymentResponse.cs
│   ├── AddTuitionRequest.cs
│   ├── TransactionResponse.cs
│   ├── UnpaidTuitionResponse.cs
│   └── LoginRequest.cs & LoginResponse.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Middleware/
│   └── RequestResponseLoggingMiddleware.cs
├── Program.cs
├── appsettings.json
├── azure-deploy.md
├── web.config
└── sample-tuitions.csv
```

---

## Requirements Checklist

### Functional Requirements
- ✅ University Mobile App - Query Tuition (rate limited to 3/day)
- ✅ Banking App - Query Tuition (with auth)
- ✅ Banking App - Pay Tuition (no auth, partial payment support)
- ✅ Admin - Add Tuition (with auth)
- ✅ Admin - Batch Add Tuition from CSV (with auth)
- ✅ Admin - Unpaid Tuition Status with paging (with auth)

### Technical Requirements
- ✅ REST API with versioning (v1)
- ✅ JWT Authentication for Banking and Admin
- ✅ Paging support (Unpaid Tuition Status)
- ✅ Rate limiting (Mobile App)
- ✅ API Gateway-ready architecture
- ✅ Comprehensive logging (request/response details)
- ✅ Swagger UI documentation
- ✅ Database service integration (SQL Server/Azure SQL)
- ✅ Azure deployment configuration
- ✅ No frontend required

### Deliverables
- ✅ Source code with proper structure
- ✅ README with design, assumptions, issues
- ✅ Data model (ER diagram)
- ✅ Azure deployment ready
- ⏳ Video presentation link (add below)

---

## Video Presentation

**Project Demonstration Video:**
[Add your video link here - YouTube, Google Drive, or other platform]

The video should cover:
- Project overview and architecture
- API endpoints demonstration
- Authentication flow
- Rate limiting demonstration
- Batch upload functionality
- Deployed application on Azure
- Swagger UI walkthrough

---

## Local Development

### Prerequisites
- .NET 8.0 SDK
- SQL Server or LocalDB
- Visual Studio 2022 or VS Code

### Running Locally

1. **Clone the repository:**
```bash
git clone <your-repo-url>
cd UniversityTuitionAPI
```

2. **Restore packages:**
```bash
dotnet restore
```

3. **Update connection string** in `appsettings.json` if needed

4. **Run the application:**
```bash
dotnet run
```

5. **Access Swagger UI:**
```
https://localhost:5001
```

### Database Initialization
- Database is auto-created on first run
- Sample data is automatically seeded
- Uses EnsureCreated() for development

---

## Future Enhancements

1. **API Gateway Integration**
   - Implement Azure API Management
   - Centralized rate limiting
   - Advanced monitoring and analytics

2. **Enhanced Security**
   - OAuth 2.0 integration
   - API key authentication for mobile apps
   - Rate limiting per API key

3. **Performance Optimization**
   - Redis caching for frequently queried data
   - Database query optimization
   - Response compression

4. **Additional Features**
   - Email notifications for payments
   - Payment history endpoint
   - Student registration API
   - Receipt generation

---

## License
This project is for educational purposes as part of SE 4458 course requirements.

---

## Contact
For questions or issues, please contact through the course platform or create an issue in the repository.
