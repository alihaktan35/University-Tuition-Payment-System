# Azure Deployment Guide

## Prerequisites
- Azure Account with active subscription
- Azure CLI installed (`az --version`)
- .NET 9.0 SDK installed
- Git repository access

## Architecture Overview
```
Internet → Azure API Gateway (App Service) → Azure API (App Service) → Azure SQL Database
```

## Step-by-Step Deployment

### Step 1: Create Azure SQL Database

```bash
# Login to Azure
az login

# Create resource group
az group create --name TuitionPaymentRG --location eastus

# Create SQL Server
az sql server create \
  --name tuitionpayment-sql-server \
  --resource-group TuitionPaymentRG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "YourSecurePassword123!"

# Configure firewall to allow Azure services
az sql server firewall-rule create \
  --resource-group TuitionPaymentRG \
  --server tuitionpayment-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create \
  --resource-group TuitionPaymentRG \
  --server tuitionpayment-sql-server \
  --name TuitionPaymentDb \
  --service-objective S0
```

**Connection String:**
```
Server=tcp:tuitionpayment-sql-server.database.windows.net,1433;Database=TuitionPaymentDb;User ID=sqladmin;Password=YourSecurePassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### Step 2: Deploy Main API (TuitionPaymentAPI)

```bash
# Create App Service Plan
az appservice plan create \
  --name TuitionPaymentPlan \
  --resource-group TuitionPaymentRG \
  --location eastus \
  --sku B1 \
  --is-linux

# Create Web App for API
az webapp create \
  --name tuitionpayment-api \
  --resource-group TuitionPaymentRG \
  --plan TuitionPaymentPlan \
  --runtime "DOTNET:9.0"

# Configure application settings
az webapp config appsettings set \
  --name tuitionpayment-api \
  --resource-group TuitionPaymentRG \
  --settings \
    ConnectionStrings__DefaultConnection="Server=tcp:tuitionpayment-sql-server.database.windows.net,1433;Database=TuitionPaymentDb;User ID=sqladmin;Password=YourSecurePassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    JwtSettings__SecretKey="REPLACE_WITH_SECURE_32_CHAR_KEY_HERE" \
    JwtSettings__Issuer="TuitionPaymentAPI" \
    JwtSettings__Audience="TuitionPaymentClients" \
    JwtSettings__ExpirationHours="24" \
    RateLimiting__MaxRequestsPerDay="3"

# Deploy API
cd src/TuitionPaymentAPI
dotnet publish -c Release -o ./publish
cd publish
zip -r ../api-deploy.zip .
az webapp deployment source config-zip \
  --resource-group TuitionPaymentRG \
  --name tuitionpayment-api \
  --src ../api-deploy.zip
```

**API URL:** `https://tuitionpayment-api.azurewebsites.net`

### Step 3: Deploy API Gateway

```bash
# Create Web App for Gateway
az webapp create \
  --name tuitionpayment-gateway \
  --resource-group TuitionPaymentRG \
  --plan TuitionPaymentPlan \
  --runtime "DOTNET:9.0"

# Configure application settings
az webapp config appsettings set \
  --name tuitionpayment-gateway \
  --resource-group TuitionPaymentRG \
  --settings \
    ConnectionStrings__DefaultConnection="Server=tcp:tuitionpayment-sql-server.database.windows.net,1433;Database=TuitionPaymentDb;User ID=sqladmin;Password=YourSecurePassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
    RateLimiting__MaxRequestsPerDay="3" \
    ReverseProxy__Clusters__tuition-api-cluster__Destinations__destination1__Address="https://tuitionpayment-api.azurewebsites.net"

# Deploy Gateway
cd ../../../src/APIGateway
dotnet publish -c Release -o ./publish
cd publish
zip -r ../gateway-deploy.zip .
az webapp deployment source config-zip \
  --resource-group TuitionPaymentRG \
  --name tuitionpayment-gateway \
  --src ../gateway-deploy.zip
```

**Gateway URL:** `https://tuitionpayment-gateway.azurewebsites.net`

### Step 4: Run Database Migrations

```bash
# From local machine, update connection string temporarily
# In src/TuitionPaymentAPI/appsettings.json, set the Azure SQL connection string

# Run migrations against Azure SQL
cd src/TuitionPaymentAPI
dotnet ef database update --connection "Server=tcp:tuitionpayment-sql-server.database.windows.net,1433;Database=TuitionPaymentDb;User ID=sqladmin;Password=YourSecurePassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Run Gateway migrations
cd ../APIGateway
dotnet ef database update --context GatewayDbContext --connection "Server=tcp:tuitionpayment-sql-server.database.windows.net,1433;Database=TuitionPaymentDb;User ID=sqladmin;Password=YourSecurePassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Step 5: Update Swagger Configuration

Update `src/TuitionPaymentAPI/Program.cs` line 70:
```csharp
options.AddServer(new OpenApiServer
{
    Url = "https://tuitionpayment-gateway.azurewebsites.net",
    Description = "API Gateway (Azure Production)"
});
```

Redeploy API after this change.

### Step 6: Verify Deployment

Test these URLs:

1. **Gateway Health:** `https://tuitionpayment-gateway.azurewebsites.net/`
2. **Swagger UI:** `https://tuitionpayment-gateway.azurewebsites.net/swagger`
3. **Login Endpoint:** `https://tuitionpayment-gateway.azurewebsites.net/api/v1/auth/login`

## Testing Guide

### 1. Test Login (Get JWT Token)
```bash
curl -X POST https://tuitionpayment-gateway.azurewebsites.net/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

### 2. Test Mobile Query (Rate Limited)
```bash
curl https://tuitionpayment-gateway.azurewebsites.net/api/v1/tuition/query/20210001
```

### 3. Test Banking Query (Authenticated)
```bash
curl https://tuitionpayment-gateway.azurewebsites.net/api/v1/banking/tuition/20210001 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Test Rate Limiting
```bash
# Run 4 times - 4th should return 429
for i in {1..4}; do
  curl https://tuitionpayment-gateway.azurewebsites.net/api/v1/tuition/query/20210001
  echo ""
done
```

## Monitoring & Logs

### View Application Logs
```bash
# API logs
az webapp log tail \
  --name tuitionpayment-api \
  --resource-group TuitionPaymentRG

# Gateway logs
az webapp log tail \
  --name tuitionpayment-gateway \
  --resource-group TuitionPaymentRG
```

### Enable Application Insights (Optional)
```bash
# Create Application Insights
az monitor app-insights component create \
  --app tuitionpayment-insights \
  --location eastus \
  --resource-group TuitionPaymentRG

# Link to web apps
az webapp config appsettings set \
  --name tuitionpayment-api \
  --resource-group TuitionPaymentRG \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="YOUR_CONNECTION_STRING"
```

## Security Checklist

- [ ] Change default passwords for SQL Server
- [ ] Generate secure JWT secret key (32+ characters)
- [ ] Restrict CORS in production (update Program.cs)
- [ ] Enable HTTPS only (disable HTTP)
- [ ] Configure firewall rules for SQL Server
- [ ] Enable Azure Key Vault for secrets
- [ ] Set up custom domain with SSL certificate
- [ ] Enable Azure DDoS Protection
- [ ] Configure rate limiting at Application Gateway level

## Cost Estimate

| Service | SKU | Monthly Cost |
|---------|-----|--------------|
| App Service Plan (B1) | Basic | ~$13 |
| SQL Database (S0) | Standard | ~$15 |
| **Total** | | **~$28/month** |

*Note: Free tier options available for development*

## Troubleshooting

### Issue: Database migrations fail
**Solution:** Ensure firewall allows your IP:
```bash
az sql server firewall-rule create \
  --resource-group TuitionPaymentRG \
  --server tuitionpayment-sql-server \
  --name MyLocalIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP
```

### Issue: 500 Internal Server Error
**Solution:** Check application logs and verify connection strings

### Issue: Rate limiting not working
**Solution:** Verify Gateway database migrations applied successfully

## Clean Up Resources

```bash
# Delete entire resource group (WARNING: This deletes everything)
az group delete --name TuitionPaymentRG --yes
```

## Alternative: Deploy Using Visual Studio

1. Right-click project → Publish
2. Select Azure → Azure App Service (Linux)
3. Create new App Service
4. Configure settings
5. Publish

## Alternative: Deploy Using GitHub Actions

See `.github/workflows/azure-deploy.yml` for CI/CD pipeline configuration.

---

**After deployment, update README.md with live URLs!**
