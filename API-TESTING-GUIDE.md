# API Testing Guide

## Quick Start Testing

### Step 1: Start the Application

```bash
dotnet run
```

The API will be available at: `https://localhost:5001`
Swagger UI will open automatically in your browser.

---

## Testing Scenarios

### Scenario 1: Admin Login and Add Tuition

**1.1 Login as Admin**

```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "role": "Admin"
}
```

**1.2 Add Tuition (use token from above)**

```bash
curl -X POST https://localhost:5001/api/v1/admin/tuition \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "studentNo": "2021010",
    "term": "2024-Fall",
    "amount": 15000
  }'
```

**Expected Response:**
```json
{
  "status": "Success",
  "message": "Tuition added successfully for student 2021010",
  "recordsProcessed": 1
}
```

---

### Scenario 2: Mobile App - Query Tuition (Rate Limited)

**2.1 First Query (Success)**

```bash
curl https://localhost:5001/api/v1/mobile/tuition/2021001
```

**Expected Response:**
```json
{
  "studentNo": "2021001",
  "tuitionTotal": 30000.00,
  "balance": 0.00,
  "isPaid": true
}
```

**2.2 Repeat 3 times to test rate limiting**

Run the same command 3 times. The 4th time should fail:

```bash
# 4th request
curl https://localhost:5001/api/v1/mobile/tuition/2021001
```

**Expected Response (4th request):**
```json
{
  "message": "Rate limit exceeded. Maximum 3 requests per day allowed.",
  "error": "TooManyRequests"
}
```

**HTTP Status:** 429 Too Many Requests

---

### Scenario 3: Banking - Query and Pay Tuition

**3.1 Login as Banking User**

```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "banking",
    "password": "banking123"
  }'
```

**3.2 Query Tuition (with auth)**

```bash
curl https://localhost:5001/api/v1/banking/tuition/2021002 \
  -H "Authorization: Bearer YOUR_BANKING_TOKEN"
```

**Expected Response:**
```json
{
  "studentNo": "2021002",
  "tuitionTotal": 30000.00,
  "balance": 30000.00,
  "isPaid": false
}
```

**3.3 Make Partial Payment (no auth required)**

```bash
curl -X POST https://localhost:5001/api/v1/banking/payment \
  -H "Content-Type: application/json" \
  -d '{
    "studentNo": "2021002",
    "term": "2024-Fall",
    "amount": 7500
  }'
```

**Expected Response:**
```json
{
  "status": "Partial",
  "amountPaid": 7500.00,
  "remainingBalance": 7500.00,
  "message": "Partial payment processed. Remaining balance: $7,500.00"
}
```

**3.4 Complete Payment**

```bash
curl -X POST https://localhost:5001/api/v1/banking/payment \
  -H "Content-Type: application/json" \
  -d '{
    "studentNo": "2021002",
    "term": "2024-Fall",
    "amount": 7500
  }'
```

**Expected Response:**
```json
{
  "status": "Successful",
  "amountPaid": 7500.00,
  "remainingBalance": 0.00,
  "message": "Payment processed successfully. Tuition fully paid."
}
```

---

### Scenario 4: Admin - Batch Upload

**4.1 Create a test CSV file**

Create `test-batch.csv`:
```csv
StudentNo,Term,Amount
2021020,2024-Fall,15000.00
2021021,2024-Fall,15000.00
2021022,2025-Spring,16000.00
```

**4.2 Upload CSV (with admin token)**

Using curl:
```bash
curl -X POST https://localhost:5001/api/v1/admin/tuition/batch \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -F "file=@test-batch.csv"
```

Or use Swagger UI:
1. Click "Try it out" on `/api/v1/admin/tuition/batch`
2. Click "Authorize" and enter your admin token
3. Choose your CSV file
4. Execute

**Expected Response:**
```json
{
  "status": "Success",
  "message": "Batch processing complete. 3 records added successfully.",
  "recordsProcessed": 3
}
```

---

### Scenario 5: Admin - Query Unpaid Tuitions with Paging

**5.1 Get First Page**

```bash
curl "https://localhost:5001/api/v1/admin/tuition/unpaid/2024-Fall?pageNumber=1&pageSize=5" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Expected Response:**
```json
{
  "term": "2024-Fall",
  "students": [
    {
      "studentNo": "2021002",
      "name": "Ayşe Kaya",
      "tuitionAmount": 15000.00,
      "balance": 15000.00
    },
    {
      "studentNo": "2021003",
      "name": "Mehmet Demir",
      "tuitionAmount": 15000.00,
      "balance": 7500.00
    }
  ],
  "totalCount": 4,
  "pageNumber": 1,
  "pageSize": 5,
  "totalPages": 1
}
```

**5.2 Get Second Page**

```bash
curl "https://localhost:5001/api/v1/admin/tuition/unpaid/2024-Fall?pageNumber=2&pageSize=2" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

---

### Scenario 6: Error Cases

**6.1 Unauthorized Access (no token)**

```bash
curl https://localhost:5001/api/v1/banking/tuition/2021001
```

**Expected Response:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

**6.2 Invalid Credentials**

```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "wrongpassword"
  }'
```

**Expected Response:**
```json
{
  "message": "Invalid username or password"
}
```

**6.3 Student Not Found**

```bash
curl https://localhost:5001/api/v1/mobile/tuition/9999999
```

**Expected Response:**
```json
{
  "message": "No tuition information found for student 9999999"
}
```

**HTTP Status:** 404 Not Found

**6.4 Duplicate Tuition**

```bash
curl -X POST https://localhost:5001/api/v1/admin/tuition \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -d '{
    "studentNo": "2021001",
    "term": "2024-Fall",
    "amount": 15000
  }'
```

**Expected Response:**
```json
{
  "status": "Error",
  "message": "Tuition already exists for student 2021001 in term 2024-Fall"
}
```

**6.5 Payment Exceeds Balance**

```bash
curl -X POST https://localhost:5001/api/v1/banking/payment \
  -H "Content-Type: application/json" \
  -d '{
    "studentNo": "2021002",
    "term": "2024-Fall",
    "amount": 50000
  }'
```

**Expected Response:**
```json
{
  "status": "Error",
  "message": "Payment amount exceeds remaining balance of $15,000.00"
}
```

---

## Testing with Swagger UI

### Accessing Swagger

1. Navigate to: `https://localhost:5001`
2. Swagger UI will load automatically

### Using Authentication in Swagger

1. Click the **"Authorize"** button (lock icon) at the top right
2. Use the `/api/v1/auth/login` endpoint to get a token
3. Copy the token value (just the token string, not the quotes)
4. In the Authorize dialog, enter: `Bearer YOUR_TOKEN`
5. Click "Authorize"
6. Now you can test protected endpoints

### Testing Each Endpoint

1. Expand the endpoint you want to test
2. Click "Try it out"
3. Fill in the required parameters
4. Click "Execute"
5. View the response below

---

## Postman Collection

### Import into Postman

1. Create a new Collection: "University Tuition API"
2. Add requests for each endpoint
3. Set base URL as a variable: `{{baseUrl}}` = `https://localhost:5001`
4. Set token as a variable: `{{token}}`

### Sample Postman Request Structure

**Environment Variables:**
```
baseUrl: https://localhost:5001
token: (leave empty, will be set after login)
```

**Pre-request Script for Protected Endpoints:**
```javascript
pm.request.headers.add({
    key: 'Authorization',
    value: 'Bearer ' + pm.environment.get('token')
});
```

**Test Script for Login:**
```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.environment.set('token', jsonData.token);
}
```

---

## Database Verification

### Check Logs

```sql
-- View recent API logs
SELECT TOP 10
    RequestTimestamp,
    HttpMethod,
    RequestPath,
    StatusCode,
    ResponseLatencyMs,
    AuthenticationSucceeded
FROM ApiLogs
ORDER BY RequestTimestamp DESC;
```

### Check Rate Limits

```sql
-- View rate limit records
SELECT
    StudentNo,
    Endpoint,
    RequestDate,
    RequestCount
FROM RateLimits
WHERE RequestDate >= CAST(GETDATE() AS DATE);
```

### Check Payments

```sql
-- View recent payments
SELECT TOP 10
    p.StudentNo,
    p.Term,
    p.Amount,
    p.Status,
    p.PaymentDate,
    t.Balance as RemainingBalance
FROM Payments p
INNER JOIN Tuitions t ON p.TuitionId = t.Id
ORDER BY p.PaymentDate DESC;
```

---

## Performance Testing

### Using Apache Bench (ab)

Test rate limiting:
```bash
ab -n 10 -c 1 https://localhost:5001/api/v1/mobile/tuition/2021001
```

### Expected Behavior
- First 3 requests: HTTP 200
- Remaining requests: HTTP 429

---

## Troubleshooting

### SSL Certificate Errors in Development

If using curl and getting SSL errors:
```bash
curl -k https://localhost:5001/api/v1/mobile/tuition/2021001
```

(The `-k` flag bypasses SSL verification for development)

### Connection String Issues

If database connection fails, check:
1. SQL Server is running
2. Connection string in `appsettings.json`
3. Firewall settings

### Authentication Issues

If getting 401 errors:
1. Ensure token is not expired (24-hour validity)
2. Check token format: `Bearer <token>`
3. Verify role permissions

---

## Default Test Data

**Users:**
- Username: `admin`, Password: `admin123`, Role: Admin
- Username: `banking`, Password: `banking123`, Role: Banking

**Students:**
- 2021001 - Ahmet Yılmaz (Fully paid 2024-Fall)
- 2021002 - Ayşe Kaya (Unpaid)
- 2021003 - Mehmet Demir (Partially paid 2024-Fall)
- 2021004 - Fatma Şahin (Unpaid)
- 2021005 - Ali Çelik (Unpaid)

**Terms:**
- 2024-Fall
- 2025-Spring

**Tuition Amount:**
- 15,000.00 per term
