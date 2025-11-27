# Local Development Guide

## Important: Database Configuration

The project has been configured to work on **all platforms** (Windows, Mac, Linux).

### Development Database (Mac/Linux)
- **Database**: SQLite
- **File**: `UniversityTuition.db` (created automatically)
- **Connection String**: `Data Source=UniversityTuition.db`

### Production Database (Azure)
- **Database**: Azure SQL Database / SQL Server
- **Connection String**: Set in Azure App Service configuration

## How It Works

The application automatically detects which database to use:

```csharp
// In Program.cs
if (connectionString.Contains("Data Source=") && connectionString.EndsWith(".db"))
{
    // Use SQLite for local development
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    // Use SQL Server for Azure production
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

## Running Locally

### 1. Clean Start
```bash
# Navigate to project directory
cd /Users/ahs/Downloads/se4458_midterm

# Clean previous builds
dotnet clean

# Restore packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run
```

### 2. Access the API
Once running, open your browser to:
- **Swagger UI**: http://localhost:5000
- **HTTPS**: https://localhost:5001 (if configured)

### 3. Database File
- The SQLite database file `UniversityTuition.db` will be created automatically
- Located in the project root directory
- Contains all tables and sample data

## Testing the API

### Quick Test via Swagger
1. Navigate to http://localhost:5000
2. Click "Authorize" button
3. Login using `/api/v1/auth/login`:
   ```json
   {
     "username": "admin",
     "password": "admin123"
   }
   ```
4. Copy the token
5. Click "Authorize" again and paste: `Bearer YOUR_TOKEN`
6. Test any endpoint

### Quick Test via curl

**Login:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

**Query Tuition (Mobile - No Auth):**
```bash
curl http://localhost:5000/api/v1/mobile/tuition/2021001
```

**Query Tuition (Banking - With Auth):**
```bash
curl http://localhost:5000/api/v1/banking/tuition/2021001 \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Stopping the Application

Press `Ctrl+C` in the terminal where dotnet run is running.

## Database Reset

To reset the database (delete all data):
```bash
# Stop the application first (Ctrl+C)
rm UniversityTuition.db
dotnet run
# Database will be recreated with fresh sample data
```

## Troubleshooting

### Port Already in Use
If you get "port already in use" error:
```bash
# Find and kill the process using port 5000
lsof -ti:5000 | xargs kill -9
```

### Database Locked
If you get "database is locked" error:
```bash
# Stop the application (Ctrl+C)
# Delete the database file
rm UniversityTuition.db
# Run again
dotnet run
```

### Package Restore Issues
```bash
dotnet clean
dotnet restore --force
dotnet build
```

## Differences from Production

| Feature | Local (SQLite) | Azure (SQL Server) |
|---------|----------------|-------------------|
| Database | SQLite file | Azure SQL Database |
| Connection | File-based | Server-based |
| Performance | Good for dev | Optimized for production |
| Features | Limited advanced features | Full T-SQL support |
| Cost | Free | Azure pricing |

## Important Notes

1. **SQLite Limitations**:
   - Some advanced SQL Server features may not be available
   - Good enough for development and testing
   - All core functionality works perfectly

2. **Azure Deployment**:
   - Will automatically use SQL Server when deployed
   - Configure connection string in Azure App Service
   - See `azure-deploy.md` for deployment instructions

3. **Sample Data**:
   - Both SQLite and SQL Server use the same seed data
   - 5 students with tuition records
   - 2 users (admin, banking)

## .NET Version

This project uses **.NET 9.0**

To verify your .NET version:
```bash
dotnet --version
```

Expected output: `9.0.x`

If you need to install .NET 9.0, download from:
https://dotnet.microsoft.com/download/dotnet/9.0

## Next Steps

1. ✅ Run the application locally
2. ✅ Test all endpoints using Swagger
3. ✅ Test rate limiting (3 requests/day for mobile)
4. ✅ Test authentication
5. ✅ Test payment processing
6. ✅ Test batch CSV upload
7. ✅ Push to GitHub
8. ⏳ Deploy to Azure (use SQL Server)
9. ⏳ Create video presentation
10. ⏳ Submit project

## Ready to Run!

Your project is now ready to run on Mac with SQLite for local development:

```bash
dotnet run
```

Then open: **http://localhost:5000**

Enjoy! 🚀
