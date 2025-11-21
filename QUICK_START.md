# Quick Start Guide - University Tuition Payment System

## 🚀 Start the System (Local Development)

### Terminal 1 - Start Main API
```bash
cd src/TuitionPaymentAPI
dotnet run
```
**Listens on:** `http://localhost:5001`

### Terminal 2 - Start API Gateway
```bash
cd src/APIGateway
dotnet run
```
**Listens on:** `http://localhost:5000` ⭐ **USE THIS ONE**

### Terminal 3 - Access Swagger UI
**Open browser:** http://localhost:5000/swagger

---

## ⚡ Quick Test Commands

### 1. Login (Get JWT Token)
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

**Save the token from response!**

### 2. Query Tuition (Mobile - No Auth)
```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
```

### 3. Test Rate Limiting (4th call should fail)
```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
curl http://localhost:5000/api/v1/tuition/query/20210001
curl http://localhost:5000/api/v1/tuition/query/20210001
curl http://localhost:5000/api/v1/tuition/query/20210001  # Should return 429
```

### 4. Query with Authentication
```bash
# Replace YOUR_TOKEN with actual token
curl http://localhost:5000/api/v1/banking/tuition/20210001 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 5. Get Unpaid List (Admin)
```bash
curl "http://localhost:5000/api/v1/admin/unpaid/2024-Fall?page=1&pageSize=5" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 📝 Test Users

| Username | Password | Role |
|----------|----------|------|
| admin | Admin123! | Admin |
| bankapi | Bank123! | BankingSystem |

## 📊 Test Students

| StudentNo | Name | Balance | Status |
|-----------|------|---------|--------|
| 20210001 | Ahmet Yılmaz | 50000 | UNPAID |
| 20210002 | Ayşe Demir | 25000 | PARTIAL |
| 20210003 | Mehmet Kaya | 0 | PAID |

---

## 🔍 What to Check in Gateway Logs

### Request Logs Should Show:
- ✅ HTTP Method (GET/POST)
- ✅ Full Path with query string
- ✅ Timestamp (UTC)
- ✅ Source IP
- ✅ Headers
- ✅ Authorization Header status
- ✅ Request size

### Response Logs Should Show:
- ✅ Status Code
- ✅ Response Latency (ms)
- ✅ Response Size
- ✅ Authentication Status (detailed)
- ✅ Rate Limit status (if exceeded)

### Example Log Output:
```
=== API GATEWAY REQUEST [a1b2c3d4] ===
HTTP Method: GET
Full Path: /api/v1/tuition/query/20210001
Timestamp: 2025-11-21 17:30:45.123 UTC
Source IP: ::1
Authorization Header: Not Present
Request Size: 0 bytes

=== API GATEWAY RESPONSE [a1b2c3d4] ===
Status Code: 200
Response Latency: 45 ms
Response Size: 128 bytes
Authentication Status: N/A - Public Endpoint
```

---

## 📚 Full Documentation

- **Complete Testing**: See [TEST_ALL_ENDPOINTS.md](TEST_ALL_ENDPOINTS.md)
- **Azure Deployment**: See [AZURE_DEPLOYMENT_GUIDE.md](AZURE_DEPLOYMENT_GUIDE.md)
- **All Changes**: See [CHANGES_SUMMARY.md](CHANGES_SUMMARY.md)
- **Main README**: See [README.md](README.md)

---

## 🎯 Requirements Checklist

- [x] 6 API Endpoints working
- [x] Rate limiting AT GATEWAY LEVEL
- [x] Comprehensive Gateway logging
- [x] Swagger points to Gateway
- [x] JWT Authentication
- [x] Role-based Authorization
- [x] Paging support
- [x] Azure deployment ready

---

## ⚠️ Important Notes

1. **Always use port 5000 (Gateway)** - Never access port 5001 directly
2. **Check Gateway terminal** for request/response logs
3. **Rate limiting resets** at midnight UTC
4. **JWT tokens expire** after 24 hours
5. **Database auto-creates** on first run

---

## 🐛 Troubleshooting

### Port already in use?
```bash
# Find process using port 5000 or 5001
lsof -i :5000
lsof -i :5001

# Kill process
kill -9 <PID>
```

### Database issues?
```bash
# Delete and recreate databases
rm src/TuitionPaymentAPI/TuitionPaymentDb.db
rm src/APIGateway/GatewayDb.db

# Restart both services - databases will auto-create
```

### Can't login?
- Username: `admin`
- Password: `Admin123!` (case-sensitive)
- Make sure request has `Content-Type: application/json` header

---

**Ready to test! 🚀**

**For video recording, demonstrate:**
1. Gateway logs showing all required fields
2. Rate limiting (4th request fails)
3. Authentication (login → use token → test protected endpoint)
4. All 6 endpoints working
5. Swagger UI with Gateway URL
