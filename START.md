# 🚀 Quick Start Guide

## Running the Application

You need to run **both** applications simultaneously in separate terminal windows.

### Terminal 1 - Main API (Port 5001)

```bash
cd /Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/TuitionPaymentAPI
dotnet run
```

**Wait** until you see: `Now listening on: http://localhost:5001`

### Terminal 2 - API Gateway (Port 5000)

```bash
cd /Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/APIGateway
dotnet run
```

**Wait** until you see: `Now listening on: http://localhost:5000`

---

## 🌐 Access the Application

Once both are running, open your browser:

- **API Documentation (Scalar UI)**: http://localhost:5000/scalar/v1
- **API Gateway**: http://localhost:5000
- **Direct API Access**: http://localhost:5001/scalar/v1 (for testing only)

---

## 🧪 Quick Test

### 1. Test Authentication

```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

Copy the `token` from the response.

### 2. Test Mobile Endpoint (No Auth)

```bash
curl http://localhost:5000/api/v1/tuition/query/20210001
```

### 3. Test Payment

```bash
curl -X POST http://localhost:5000/api/v1/banking/pay \
  -H "Content-Type: application/json" \
  -d '{"studentNo":"20210001","term":"2024-Fall","amount":10000}'
```

### 4. Test Admin Endpoint (With Auth)

Replace `YOUR_TOKEN` with the token from step 1:

```bash
curl http://localhost:5000/api/v1/admin/unpaid/2024-Fall \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 5. Test Rate Limiting

Run this 4 times rapidly - the 4th call should return HTTP 429:

```bash
for i in {1..4}; do
  echo "Request $i:"
  curl http://localhost:5000/api/v1/tuition/query/20210001
  echo -e "\n"
done
```

---

## 📊 Pre-seeded Test Data

### Users
- **Username**: `admin`, **Password**: `Admin123!`, **Role**: Admin
- **Username**: `bankapi`, **Password**: `Bank123!`, **Role**: BankingSystem

### Students
- **20210001**: Ahmet Yılmaz - 50,000 TRY (UNPAID)
- **20210002**: Ayşe Demir - 25,000 TRY remaining (PARTIAL)
- **20210003**: Mehmet Kaya - 0 TRY remaining (PAID)
- **20210004**: Fatma Öz - 50,000 TRY (UNPAID)

---

## ❗ Troubleshooting

### Issue: "Connection refused (localhost:5001)"
**Solution**: Make sure the Main API (Terminal 1) is running first and showing "Now listening on: http://localhost:5001"

### Issue: Port already in use
**Solution**:
```bash
# Find and kill process on port 5000 or 5001
lsof -ti:5000 | xargs kill -9
lsof -ti:5001 | xargs kill -9
```

### Issue: Database errors
**Solution**: Delete the database and restart:
```bash
rm /Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/TuitionPaymentAPI/TuitionPaymentDb.db
# Then run the API again - it will recreate and seed the database
```

---

## 📝 Important Notes

1. **Always access through Gateway** (port 5000) in production
2. **API Gateway logs** all requests in the console
3. **Rate limiting** resets daily at midnight UTC
4. **JWT tokens** expire after 24 hours
5. The **Scalar UI** is better than Swagger - it's the modern standard for .NET 9

---

## 🎥 For Your Video Demo

Show these features:

1. ✅ **Architecture**: Two applications running (API + Gateway)
2. ✅ **Scalar UI**: Modern API documentation interface
3. ✅ **Authentication**: Login and get JWT token
4. ✅ **Rate Limiting**: Call mobile endpoint 4 times (show 429 error)
5. ✅ **Partial Payment**: Make payment and see balance update
6. ✅ **Gateway Logging**: Show console logs with all request details
7. ✅ **Pagination**: Query unpaid tuition with page parameters
8. ✅ **CSV Upload**: Use Scalar UI to upload a CSV file

---

## 🎓 Good Luck!

Your project is complete and ready to demonstrate. All requirements from SE 4458 are implemented!
