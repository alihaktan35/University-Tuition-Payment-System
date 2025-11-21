# Complete Testing Guide - All Endpoints Through Gateway

## ⚠️ IMPORTANT: All tests MUST go through API Gateway (Port 5000)

### Prerequisites

1. Start API (Terminal 1):
```bash
cd src/TuitionPaymentAPI
dotnet run
```

2. Start Gateway (Terminal 2):
```bash
cd src/APIGateway
dotnet run
```

3. Wait for both to start (should see "Now listening on: http://localhost:XXXX")

---

## Test 1: Authentication - Login

### Test 1.1: Admin Login ✅
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 86400,
  "username": "admin",
  "role": "Admin"
}
```

**Save the token for next tests!**

### Test 1.2: Banking System Login ✅
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"bankapi","password":"Bank123!"}'
```

---

## Test 2: Mobile App - Query Tuition (NO AUTH, RATE LIMITED)

### Test 2.1: First Query (Should Succeed) ✅
```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
```

**Expected Response:**
```json
{
  "tuitionTotal": 50000.00,
  "balance": 50000.00
}
```

**Check Gateway Logs for:**
- ✅ Rate limit check logged
- ✅ Count: 1/3

### Test 2.2: Second Query (Should Succeed) ✅
```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
```

**Check Gateway Logs for:**
- ✅ Count: 2/3

### Test 2.3: Third Query (Should Succeed) ✅
```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
```

**Check Gateway Logs for:**
- ✅ Count: 3/3

### Test 2.4: Fourth Query (Should FAIL - Rate Limit Exceeded) ✅
```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
```

**Expected Response (HTTP 429):**
```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "You have exceeded the maximum of 3 requests per day",
    "details": {
      "studentNo": "20210001",
      "callsToday": 3,
      "maxAllowed": 3,
      "resetTime": "2025-11-22T00:00:00Z"
    }
  }
}
```

**Check Gateway Logs for:**
- ⚠️ Rate limit EXCEEDED
- ✅ HTTP 429 response

### Test 2.5: Different Student (Should Succeed) ✅
```bash
curl http://localhost:5000/api/v1/tuition/query/20210002
```

**Should work** - Rate limiting is per-student

---

## Test 3: Banking App - Query Tuition (AUTHENTICATED)

### Test 3.1: Without Token (Should FAIL) ✅
```bash
curl http://localhost:5000/api/v1/banking/tuition/20210001
```

**Expected Response (HTTP 401):**
```json
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "No authentication token provided"
  }
}
```

**Check Gateway Logs for:**
- ✅ Authentication Status: FAILED - No Token Provided

### Test 3.2: With Valid Token (Should SUCCEED) ✅
```bash
# Replace YOUR_TOKEN with the token from Test 1
curl http://localhost:5000/api/v1/banking/tuition/20210001 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response (HTTP 200):**
```json
{
  "tuitionTotal": 50000.00,
  "balance": 50000.00
}
```

**Check Gateway Logs for:**
- ✅ Authorization Header: Present (Bearer Token)
- ✅ Authentication Status: SUCCEEDED - Valid Token
- ✅ Response latency logged

### Test 3.3: With Invalid Token (Should FAIL) ✅
```bash
curl http://localhost:5000/api/v1/banking/tuition/20210001 \
  -H "Authorization: Bearer INVALID_TOKEN_HERE"
```

**Expected Response (HTTP 401):**

**Check Gateway Logs for:**
- ✅ Authentication Status: FAILED - Invalid/Expired Token

---

## Test 4: Banking App - Pay Tuition (NO AUTH REQUIRED)

### Test 4.1: Partial Payment ✅
```bash
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2024-Fall","amount":10000.00}'
```

**Expected Response (HTTP 200):**
```json
{
  "status": "Successful",
  "remainingBalance": 40000.00,
  "message": "Partial payment processed. Remaining balance: ₺40,000.00"
}
```

### Test 4.2: Full Payment ✅
```bash
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210004","term":"2024-Fall","amount":50000.00}'
```

**Expected Response (HTTP 200):**
```json
{
  "status": "Successful",
  "remainingBalance": 0.00,
  "message": "Payment completed. Tuition fully paid."
}
```

### Test 4.3: Overpayment (Should FAIL) ✅
```bash
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210002","term":"2024-Fall","amount":100000.00}'
```

**Expected Response (HTTP 400):**
```json
{
  "status": "Error",
  "message": "Payment amount (100000) exceeds remaining balance (25000)"
}
```

---

## Test 5: Admin - Add Tuition (ADMIN ROLE REQUIRED)

### Test 5.1: Without Token (Should FAIL) ✅
```bash
curl -X POST http://localhost:5000/api/v1/admin/tuition \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2025-Spring","amount":55000.00}'
```

**Expected Response (HTTP 401):**

**Check Gateway Logs for:**
- ✅ Authentication Status: FAILED - No Token for Protected Resource

### Test 5.2: With Banking Token (Should FAIL - Wrong Role) ✅
```bash
# Use token from banking login
curl -X POST http://localhost:5000/api/v1/admin/tuition \
  -H "Authorization: Bearer BANKING_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2025-Spring","amount":55000.00}'
```

**Expected Response (HTTP 403):**

**Check Gateway Logs for:**
- ✅ Authentication Status: SUCCEEDED but Insufficient Permissions (Forbidden)

### Test 5.3: With Admin Token (Should SUCCEED) ✅
```bash
# Use token from admin login
curl -X POST http://localhost:5000/api/v1/admin/tuition \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2025-Spring","amount":55000.00}'
```

**Expected Response (HTTP 201):**
```json
{
  "status": "Success",
  "tuitionId": 18,
  "message": "Tuition record created successfully"
}
```

---

## Test 6: Admin - Batch Upload CSV (ADMIN ROLE REQUIRED)

### Test 6.1: Create Test CSV File
```bash
cat > /tmp/tuition_batch.csv << 'EOF'
studentNo,term,amount
20210001,2025-Fall,60000
20210002,2025-Fall,60000
20210003,2025-Fall,60000
20210004,2025-Fall,60000
20210005,2025-Fall,60000
EOF
```

### Test 6.2: Upload CSV ✅
```bash
curl -X POST http://localhost:5000/api/v1/admin/tuition/batch \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -F "file=@/tmp/tuition_batch.csv"
```

**Expected Response (HTTP 200):**
```json
{
  "status": "Success",
  "successCount": 5,
  "errorCount": 0,
  "errors": []
}
```

### Test 6.3: Upload Invalid CSV (Should Report Errors) ✅
```bash
cat > /tmp/tuition_invalid.csv << 'EOF'
studentNo,term,amount
99999999,2025-Fall,60000
20210001,2025-Fall,INVALID
EOF

curl -X POST http://localhost:5000/api/v1/admin/tuition/batch \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -F "file=@/tmp/tuition_invalid.csv"
```

**Expected Response (HTTP 200):**
```json
{
  "status": "Partial Success",
  "successCount": 0,
  "errorCount": 2,
  "errors": [
    {"row": 1, "reason": "Student 99999999 not found"},
    {"row": 2, "reason": "Amount must be a positive number"}
  ]
}
```

---

## Test 7: Admin - Unpaid Tuition List (ADMIN ROLE, PAGINATED)

### Test 7.1: Get First Page ✅
```bash
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall?page=1&pageSize=2" \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

**Expected Response (HTTP 200):**
```json
{
  "students": [
    {
      "studentNo": "20210001",
      "name": "Ahmet Yılmaz",
      "tuitionTotal": 50000.00,
      "balance": 40000.00
    },
    {
      "studentNo": "20210002",
      "name": "Ayşe Demir",
      "tuitionTotal": 50000.00,
      "balance": 25000.00
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 2,
    "totalPages": 3,
    "totalCount": 5
  }
}
```

### Test 7.2: Get Second Page ✅
```bash
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall?page=2&pageSize=2" \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

### Test 7.3: Invalid Page (Should FAIL) ✅
```bash
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall?page=0&pageSize=2" \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

**Expected Response (HTTP 400):**
```json
{
  "error": {
    "code": "INVALID_PAGE",
    "message": "Page number must be greater than 0"
  }
}
```

---

## Test 8: Swagger UI Verification

### Test 8.1: Access Swagger ✅
Open browser: http://localhost:5000/swagger

**Verify:**
- ✅ Swagger UI loads
- ✅ All endpoints visible
- ✅ Server dropdown shows "API Gateway (Local Development)"
- ✅ Can authorize with JWT token
- ✅ All requests go to port 5000 (Gateway)

### Test 8.2: Test Login via Swagger ✅
1. Click on `/api/v1/auth/login` POST endpoint
2. Click "Try it out"
3. Enter credentials
4. Execute
5. Copy token
6. Click "Authorize" button at top
7. Enter: `Bearer YOUR_TOKEN`
8. Try authenticated endpoint

---

## Test 9: Gateway Logging Verification

### Check logs in Gateway terminal for each request:

**Request Log Should Include:**
- ✅ HTTP Method (GET/POST/etc.)
- ✅ Full Path with query string
- ✅ Timestamp (UTC)
- ✅ Source IP address
- ✅ Headers (excluding Authorization value)
- ✅ Authorization Header status (Present/Not Present)
- ✅ Request Size in bytes

**Response Log Should Include:**
- ✅ Status Code (200, 401, 403, 429, etc.)
- ✅ Response Latency in milliseconds
- ✅ Response Size in bytes
- ✅ Authentication Status:
  - "FAILED - No Token Provided" (401 without header)
  - "FAILED - Invalid/Expired Token" (401 with header)
  - "SUCCEEDED but Insufficient Permissions (Forbidden)" (403)
  - "SUCCEEDED - Valid Token" (200 with header)
  - "N/A - Public Endpoint" (200 without header)
  - "N/A - Rate Limited" (429)
- ✅ Rate Limit status when exceeded

---

## Test 10: Rate Limiting Database Verification

```bash
# View rate limit records in Gateway database
sqlite3 src/APIGateway/GatewayDb.db "SELECT * FROM RateLimits;"
```

**Expected Output:**
```
20210001|/api/v1/tuition/query/20210001|3|2025-11-21|2025-11-21 17:30:45
20210002|/api/v1/tuition/query/20210002|1|2025-11-21|2025-11-21 17:31:12
```

---

## Success Criteria Checklist

### Functionality:
- [ ] All 6 API endpoints work through Gateway
- [ ] Rate limiting enforced at Gateway level (3 requests/day)
- [ ] JWT authentication validates correctly
- [ ] Role-based authorization works (Admin vs Banking)
- [ ] Partial payments supported
- [ ] CSV batch upload works with validation
- [ ] Pagination works on unpaid list
- [ ] Error handling returns proper status codes

### Logging (Gateway):
- [ ] HTTP method logged
- [ ] Full request path with query string logged
- [ ] Request timestamp (UTC) logged
- [ ] Source IP address logged
- [ ] Headers logged (excluding sensitive data)
- [ ] Request size logged
- [ ] Authentication status detailed and accurate
- [ ] Status code logged
- [ ] Response latency logged
- [ ] Response size logged
- [ ] Rate limit exceeded events logged
- [ ] Mapping/proxy failures logged (if any)

### Technical:
- [ ] Swagger points to Gateway URL (port 5000)
- [ ] Rate limiting data persists in Gateway database
- [ ] No rate limiting middleware in API project
- [ ] Both projects build without errors
- [ ] Database migrations applied successfully

---

## Quick Test All Script

```bash
#!/bin/bash

echo "=== Quick Test All Endpoints ==="

echo "\n1. Login..."
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | grep -o '"token":"[^"]*' | cut -d'"' -f4)

echo "Token: ${TOKEN:0:20}..."

echo "\n2. Query tuition (mobile)..."
curl -s http://localhost:5000/api/v1/tuition/query/20210001 | head -n 5

echo "\n3. Query tuition (banking - authenticated)..."
curl -s http://localhost:5000/api/v1/banking/tuition/20210001 \
  -H "Authorization: Bearer $TOKEN" | head -n 5

echo "\n4. Test rate limiting (4th call should fail)..."
for i in {1..4}; do
  echo "Call $i:"
  curl -s http://localhost:5000/api/v1/tuition/query/TEST$(date +%s) | head -n 3
  echo ""
done

echo "\n5. Get unpaid list..."
curl -s "http://localhost:5000/api/v1/admin/unpaid/2024-Fall?page=1&pageSize=2" \
  -H "Authorization: Bearer $TOKEN" | head -n 10

echo "\n=== All Tests Complete ==="
```

---

**After all tests pass, proceed with Azure deployment!**
