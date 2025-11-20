# 🗄️ Database Viewer Guide

## View Your SQLite Database

The database file is located at: `/Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/TuitionPaymentAPI/TuitionPaymentDb.db`

---

## 📊 Quick View Commands

### View All Students
```bash
cd /Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/TuitionPaymentAPI
sqlite3 TuitionPaymentDb.db "SELECT * FROM Students;"
```

### View All Tuitions (with Student Names)
```bash
sqlite3 TuitionPaymentDb.db "
SELECT
    t.TuitionId,
    s.StudentNo,
    s.Name,
    t.Term,
    t.TotalAmount,
    t.Balance,
    t.PaidAmount,
    t.Status,
    t.CreatedAt
FROM Tuitions t
JOIN Students s ON t.StudentId = s.StudentId
ORDER BY t.TuitionId;
"
```

### View All Payments
```bash
sqlite3 TuitionPaymentDb.db "
SELECT
    p.PaymentId,
    s.StudentNo,
    s.Name,
    t.Term,
    p.Amount,
    p.PaymentDate,
    p.Status,
    p.TransactionReference
FROM Payments p
JOIN Tuitions t ON p.TuitionId = t.TuitionId
JOIN Students s ON t.StudentId = s.StudentId
ORDER BY p.PaymentDate DESC;
"
```

### View All Users
```bash
sqlite3 TuitionPaymentDb.db "SELECT UserId, Username, Role, CreatedAt FROM Users;"
```

### View Rate Limits
```bash
sqlite3 TuitionPaymentDb.db "SELECT * FROM RateLimits ORDER BY Date DESC, LastCall DESC;"
```

---

## 🎨 Pretty Format (Table View)

Add `-header -column` for better formatting:

```bash
sqlite3 -header -column TuitionPaymentDb.db "
SELECT
    s.StudentNo,
    s.Name,
    t.Term,
    t.TotalAmount as 'Total',
    t.Balance,
    t.PaidAmount as 'Paid',
    t.Status
FROM Tuitions t
JOIN Students s ON t.StudentId = s.StudentId
ORDER BY s.StudentNo, t.Term;
"
```

**Output Example:**
```
StudentNo  Name           Term       Total   Balance  Paid    Status
---------  -------------  ---------  ------  -------  ------  -------
20210001   Ahmet Yılmaz   2024-Fall  50000   50000    0       UNPAID
20210001   Ahmet Yılmaz   2025-Fall  60000   60000    0       UNPAID
20210002   Ayşe Demir     2024-Fall  50000   25000    25000   PARTIAL
```

---

## 🔍 Useful Queries

### Find Unpaid Tuition for a Specific Term
```bash
sqlite3 -header -column TuitionPaymentDb.db "
SELECT
    s.StudentNo,
    s.Name,
    t.Term,
    t.Balance,
    t.Status
FROM Tuitions t
JOIN Students s ON t.StudentId = s.StudentId
WHERE t.Term = '2024-Fall' AND (t.Status = 'UNPAID' OR t.Status = 'PARTIAL')
ORDER BY t.Balance DESC;
"
```

### Total Revenue by Term
```bash
sqlite3 -header -column TuitionPaymentDb.db "
SELECT
    Term,
    COUNT(*) as 'Students',
    SUM(TotalAmount) as 'Total Tuition',
    SUM(PaidAmount) as 'Total Paid',
    SUM(Balance) as 'Total Outstanding'
FROM Tuitions
GROUP BY Term;
"
```

### Student Payment History
```bash
sqlite3 -header -column TuitionPaymentDb.db "
SELECT
    s.StudentNo,
    s.Name,
    t.Term,
    p.Amount,
    p.PaymentDate,
    p.TransactionReference
FROM Payments p
JOIN Tuitions t ON p.TuitionId = t.TuitionId
JOIN Students s ON t.StudentId = s.StudentId
WHERE s.StudentNo = '20210001'
ORDER BY p.PaymentDate;
"
```

### Check Who Made Most Payments Today
```bash
sqlite3 -header -column TuitionPaymentDb.db "
SELECT
    s.StudentNo,
    s.Name,
    COUNT(*) as 'Payment Count',
    SUM(p.Amount) as 'Total Paid Today'
FROM Payments p
JOIN Tuitions t ON p.TuitionId = t.TuitionId
JOIN Students s ON t.StudentId = s.StudentId
WHERE DATE(p.PaymentDate) = DATE('now')
GROUP BY s.StudentNo
ORDER BY COUNT(*) DESC;
"
```

---

## 🖥️ Interactive Database Browser

### Option 1: Command Line (sqlite3)
```bash
cd /Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/TuitionPaymentAPI
sqlite3 TuitionPaymentDb.db
```

Then use SQL commands:
```sql
.tables                    -- List all tables
.schema Students           -- Show table structure
SELECT * FROM Students;    -- Query data
.quit                      -- Exit
```

### Option 2: DB Browser for SQLite (GUI - Recommended)

**Install:**
```bash
brew install --cask db-browser-for-sqlite
```

**Open:**
```bash
open -a "DB Browser for SQLite" TuitionPaymentDb.db
```

Or download from: https://sqlitebrowser.org/

---

## 📋 All Tables in Database

1. **Students** - Student records (StudentNo, Name, Email)
2. **Tuitions** - Tuition records per student per term
3. **Payments** - Payment transaction history
4. **Users** - Admin and banking system users
5. **RateLimits** - Rate limiting tracking for mobile endpoint

---

## 🔧 Database Schema

### View Complete Schema
```bash
sqlite3 TuitionPaymentDb.db ".schema"
```

### Export Database to SQL File
```bash
sqlite3 TuitionPaymentDb.db ".dump" > database_backup.sql
```

### Export Table to CSV
```bash
sqlite3 -header -csv TuitionPaymentDb.db "SELECT * FROM Tuitions;" > tuitions.csv
```

---

## 📸 Quick Snapshot Commands

Save these as aliases in your shell:

```bash
# Add to ~/.zshrc or ~/.bashrc:
alias db-students='cd /path/to/project/src/TuitionPaymentAPI && sqlite3 -header -column TuitionPaymentDb.db "SELECT * FROM Students;"'
alias db-tuitions='cd /path/to/project/src/TuitionPaymentAPI && sqlite3 -header -column TuitionPaymentDb.db "SELECT t.TuitionId, s.StudentNo, s.Name, t.Term, t.TotalAmount, t.Balance, t.Status FROM Tuitions t JOIN Students s ON t.StudentId = s.StudentId;"'
alias db-payments='cd /path/to/project/src/TuitionPaymentAPI && sqlite3 -header -column TuitionPaymentDb.db "SELECT p.*, s.StudentNo FROM Payments p JOIN Tuitions t ON p.TuitionId = t.TuitionId JOIN Students s ON t.StudentId = s.StudentId ORDER BY p.PaymentDate DESC LIMIT 10;"'
```

Then just run:
```bash
db-students
db-tuitions
db-payments
```

---

## 🧹 Reset Database

If you need to start fresh:

```bash
cd /Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/TuitionPaymentAPI

# Backup first (optional)
cp TuitionPaymentDb.db TuitionPaymentDb.backup.db

# Delete database
rm TuitionPaymentDb.db

# Restart the API - it will recreate and seed the database
dotnet run
```

---

## 📊 Current Database Content

Run this to see everything at once:

```bash
cd /Users/ahs/Documents/GitHub/University-Tuition-Payment-System/src/TuitionPaymentAPI

echo "=== STUDENTS ===" && \
sqlite3 -header -column TuitionPaymentDb.db "SELECT * FROM Students;" && \
echo -e "\n=== TUITIONS ===" && \
sqlite3 -header -column TuitionPaymentDb.db "SELECT t.TuitionId, s.StudentNo, s.Name, t.Term, t.TotalAmount, t.Balance, t.Status FROM Tuitions t JOIN Students s ON t.StudentId = s.StudentId;" && \
echo -e "\n=== PAYMENTS ===" && \
sqlite3 -header -column TuitionPaymentDb.db "SELECT COUNT(*) as 'Total Payments', SUM(Amount) as 'Total Amount' FROM Payments;" && \
echo -e "\n=== USERS ===" && \
sqlite3 -header -column TuitionPaymentDb.db "SELECT Username, Role FROM Users;"
```

---

**Pro Tip**: For your video demo, use the pretty table format with `-header -column` to show the database contents clearly!
