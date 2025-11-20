# 🧪 API Testing Guide

## Prerequisites

Both applications must be running:
- **Main API**: http://localhost:5001 (Terminal 1)
- **API Gateway**: http://localhost:5000 (Terminal 2)

**IMPORTANT**: All tests should be done through the **API Gateway** (port 5000), not the Main API directly!

---

## 🌐 Access Points

- **Scalar API Documentation**: http://localhost:5000/scalar/v1
- **API Gateway**: http://localhost:5000
- **OpenAPI Spec**: http://localhost:5000/openapi/v1.json

---

## Test Scenarios

### 1. Authentication Test

**Login as Admin:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400,
  "username": "admin",
  "role": "Admin"
}
```

**Copy the token** - You'll need it for authenticated requests!

**Login as Banking System:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"bankapi","password":"Bank123!"}'
```

---

### 2. Mobile App - Query Tuition (No Auth, Rate Limited)

**Test Student 20210001 (UNPAID):**
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

**Test Student 20210002 (PARTIAL):**
```bash
curl http://localhost:5000/api/v1/tuition/query/20210002
```

**Expected Response:**
```json
{
  "tuitionTotal": 50000.00,
  "balance": 25000.00
}
```

**Test Student 20210003 (PAID):**
```bash
curl http://localhost:5000/api/v1/tuition/query/20210003
```

**Expected Response:**
```json
{
  "tuitionTotal": 50000.00,
  "balance": 0.00
}
```

---

### 3. Rate Limiting Test (4th Request Should Fail)

Run this command to test rate limiting:
```bash
for i in {1..4}; do
  echo "Request $i:"
  curl -s http://localhost:5000/api/v1/tuition/query/20210001
  echo -e "\n---"
  sleep 1
done
```

**Expected Result:**
- Requests 1-3: Success (returns tuition data)
- Request 4: HTTP 429 (Too Many Requests)

**4th Request Expected Response:**
```json
{
  "error": "Rate limit exceeded",
  "message": "You have exceeded the maximum of 3 requests per day for this endpoint.",
  "retryAfter": "Midnight UTC"
}
```

---

### 4. Banking App - Query Tuition (Requires Auth)

**Replace YOUR_TOKEN with the token from step 1!**

```bash
curl http://localhost:5000/api/v1/banking/tuition/20210001 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response:**
```json
{
  "tuitionTotal": 50000.00,
  "balance": 50000.00
}
```

**Test without token (should fail):**
```bash
curl http://localhost:5000/api/v1/banking/tuition/20210001
```

**Expected Response:**
```
HTTP 401 Unauthorized
```

---

### 5. Banking App - Process Payment

**Full Payment:**
```bash
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2024-Fall","amount":50000.00}'
```

**Expected Response:**
```json
{
  "status": "Successful",
  "remainingBalance": 0.00,
  "message": "Full payment of 50000.00 processed successfully for term 2024-Fall"
}
```

**Partial Payment:**
```bash
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210004","term":"2024-Fall","amount":10000.00}'
```

**Expected Response:**
```json
{
  "status": "Successful",
  "remainingBalance": 40000.00,
  "message": "Partial payment of 10000.00 processed successfully for term 2024-Fall"
}
```

**Verify the payment updated the balance:**
```bash
curl http://localhost:5000/api/v1/tuition/query/20210004
```

---

### 6. Admin - Add Single Tuition (Requires Admin Role)

**Replace YOUR_ADMIN_TOKEN with the admin token from step 1!**

```bash
curl -X POST http://localhost:5000/api/v1/admin/tuition \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2025-Spring","amount":55000.00}'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Tuition added successfully for student 20210001, term 2025-Spring"
}
```

---

### 7. Admin - Batch Upload CSV (Requires Admin Role)

**First, create a test CSV file:**
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

**Upload the CSV:**
```bash
curl -X POST http://localhost:5000/api/v1/admin/tuition/batch \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -F "file=@/tmp/tuition_batch.csv"
```

**Expected Response:**
```json
{
  "successCount": 5,
  "errorCount": 0,
  "errors": []
}
```

**Test with invalid CSV (to see error handling):**
```bash
cat > /tmp/tuition_invalid.csv << 'EOF'
studentNo,term,amount
99999999,2025-Fall,60000
20210002,Invalid-Term,60000
20210003,2025-Fall,-1000
EOF

curl -X POST http://localhost:5000/api/v1/admin/tuition/batch \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -F "file=@/tmp/tuition_invalid.csv"
```

---

### 8. Admin - Get Unpaid Tuition with Pagination

**Get first page (default page size: 20):**
```bash
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Expected Response:**
```json
{
  "data": [
    {
      "studentNo": "20210001",
      "studentName": "Ahmet Yılmaz",
      "term": "2024-Fall",
      "totalAmount": 50000.00,
      "balance": 0.00,
      "status": "PAID"
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

**Get with custom page size:**
```bash
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall?page=1&pageSize=2" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

---

## 📊 Using Scalar UI (Recommended)

1. Open your browser to: **http://localhost:5000/scalar/v1**

2. **Authenticate:**
   - Click on "Auth" section in the left sidebar
   - Click on `/api/v1/Auth/login` (POST)
   - Click "Try it out"
   - Enter:
     ```json
     {
       "username": "admin",
       "password": "Admin123!"
     }
     ```
   - Click "Execute"
   - **Copy the token** from the response

3. **Set Authorization:**
   - Click the "Authorize" button at the top
   - Enter: `Bearer YOUR_TOKEN`
   - Click "Authorize"

4. **Test Endpoints:**
   - Navigate to any endpoint in the left sidebar
   - Click "Try it out"
   - Fill in the parameters
   - Click "Execute"

---

## 🎥 Demo Checklist

For your video demonstration, show:

1. ✅ **Architecture**: Two terminal windows running (API + Gateway)
2. ✅ **Scalar UI**: Modern API documentation at http://localhost:5000/scalar/v1
3. ✅ **Gateway Logging**: Show console logs in Gateway terminal with request details
4. ✅ **Authentication**: Login and show JWT token
5. ✅ **Rate Limiting**: Call mobile endpoint 4 times, show 429 error on 4th
6. ✅ **Partial Payment**: Make payment and query to show balance update
7. ✅ **Pagination**: Query unpaid tuition with different page sizes
8. ✅ **CSV Upload**: Use Scalar UI to upload a CSV file
9. ✅ **Role Authorization**: Try admin endpoint with and without admin role

---

## 🔍 Verify Gateway Logging

Check your **API Gateway terminal** - you should see logs like:

```
[2025-11-20 17:45:23] REQUEST: POST /api/v1/auth/login from 127.0.0.1
[2025-11-20 17:45:23] Headers: Content-Type=application/json
[2025-11-20 17:45:23] Body Size: 45 bytes
[2025-11-20 17:45:23] RESPONSE: 200 OK (125ms)
[2025-11-20 17:45:23] Response Size: 523 bytes
```

---

## ✅ Test Data Summary

### Users
- **admin** / Admin123! (Admin role)
- **bankapi** / Bank123! (BankingSystem role)

### Students
| StudentNo | Name | Tuition | Balance | Status |
|-----------|------|---------|---------|--------|
| 20210001 | Ahmet Yılmaz | 50,000 | 50,000 | UNPAID |
| 20210002 | Ayşe Demir | 50,000 | 25,000 | PARTIAL |
| 20210003 | Mehmet Kaya | 50,000 | 0 | PAID |
| 20210004 | Fatma Öz | 50,000 | 50,000 | UNPAID |
| 20210005 | Ali Şahin | No tuition yet | - | - |

---

## 🐛 Troubleshooting

### Gateway returns 502 Bad Gateway
- Main API (port 5001) is not running
- Restart the Main API first

### Database locked error
- Stop all running instances
- Delete the database file: `rm TuitionPaymentDb.db`
- Restart both applications

### Rate limit not resetting
- Rate limits reset at midnight UTC
- To manually reset: Delete the database and restart

---

**Ready to test! Start with the Scalar UI at http://localhost:5000/scalar/v1** 🚀
