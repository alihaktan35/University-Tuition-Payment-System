# Azure Deployment Guide

## Prerequisites
1. Azure Account with active subscription
2. Azure CLI installed
3. .NET 8.0 SDK installed

## Deployment Steps

### 1. Create Azure Resources

#### Create Resource Group
```bash
az group create --name UniversityTuitionRG --location eastus
```

#### Create Azure SQL Database
```bash
# Create SQL Server
az sql server create \
  --name university-tuition-sql \
  --resource-group UniversityTuitionRG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password <YourStrongPassword123!>

# Configure firewall to allow Azure services
az sql server firewall-rule create \
  --resource-group UniversityTuitionRG \
  --server university-tuition-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create \
  --resource-group UniversityTuitionRG \
  --server university-tuition-sql \
  --name UniversityTuitionDB \
  --service-objective F1 \
  --backup-storage-redundancy Local
```

#### Create App Service Plan
```bash
az appservice plan create \
  --name UniversityTuitionPlan \
  --resource-group UniversityTuitionRG \
  --location eastus \
  --sku F1 \
  --is-linux
```

#### Create Web App
```bash
az webapp create \
  --name university-tuition-api \
  --resource-group UniversityTuitionRG \
  --plan UniversityTuitionPlan \
  --runtime "DOTNET|8.0"
```

### 2. Configure Connection String

Get the SQL connection string:
```bash
az sql db show-connection-string \
  --client ado.net \
  --server university-tuition-sql \
  --name UniversityTuitionDB
```

Set it in App Service:
```bash
az webapp config connection-string set \
  --name university-tuition-api \
  --resource-group UniversityTuitionRG \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:university-tuition-sql.database.windows.net,1433;Database=UniversityTuitionDB;User ID=sqladmin;Password=<YourStrongPassword123!>;Encrypt=true;Connection Timeout=30;"
```

### 3. Configure App Settings

```bash
az webapp config appsettings set \
  --name university-tuition-api \
  --resource-group UniversityTuitionRG \
  --settings \
    Jwt__Key="<GenerateASecureRandomKey>" \
    Jwt__Issuer="UniversityTuitionAPI" \
    Jwt__Audience="UniversityTuitionAPI"
```

### 4. Deploy the Application

#### Option A: Deploy from Local (Using Azure CLI)
```bash
# Build the project
dotnet publish -c Release -o ./publish

# Create a zip file
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group UniversityTuitionRG \
  --name university-tuition-api \
  --src deploy.zip
```

#### Option B: Deploy from VS Code
1. Install Azure App Service extension
2. Right-click on the project
3. Select "Deploy to Web App"
4. Follow the prompts

### 5. Verify Deployment

Access your API at:
```
https://university-tuition-api.azurewebsites.net
```

The Swagger UI will be available at the root URL.

## API Gateway Setup (Optional - Using Azure API Management)

### Create API Management Service
```bash
az apim create \
  --name university-tuition-apim \
  --resource-group UniversityTuitionRG \
  --publisher-email your-email@example.com \
  --publisher-name "University" \
  --sku-name Consumption
```

### Configure Rate Limiting in APIM
1. Go to Azure Portal
2. Navigate to your API Management service
3. Add your API backend
4. Configure rate limiting policies in the policy editor

Example policy for rate limiting:
```xml
<policies>
    <inbound>
        <rate-limit-by-key calls="3" renewal-period="86400" counter-key="@(context.Request.Headers.GetValueOrDefault("StudentNo",""))" />
    </inbound>
</policies>
```

## Environment Variables

Set these in Azure App Service Configuration:

- `ConnectionStrings__DefaultConnection`: Your Azure SQL connection string
- `Jwt__Key`: Your JWT signing key (generate a secure random string)
- `Jwt__Issuer`: "UniversityTuitionAPI"
- `Jwt__Audience`: "UniversityTuitionAPI"

## Monitoring and Logging

Enable Application Insights:
```bash
az monitor app-insights component create \
  --app university-tuition-insights \
  --location eastus \
  --resource-group UniversityTuitionRG \
  --application-type web

# Connect to Web App
az monitor app-insights component connect-webapp \
  --app university-tuition-insights \
  --resource-group UniversityTuitionRG \
  --web-app university-tuition-api
```

## Testing the Deployed API

### Get JWT Token
```bash
curl -X POST https://university-tuition-api.azurewebsites.net/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Query Tuition (Mobile)
```bash
curl https://university-tuition-api.azurewebsites.net/api/v1/mobile/tuition/2021001
```

### Query Tuition (Banking - with auth)
```bash
curl https://university-tuition-api.azurewebsites.net/api/v1/banking/tuition/2021001 \
  -H "Authorization: Bearer <your-token>"
```

## Troubleshooting

### View Logs
```bash
az webapp log tail \
  --name university-tuition-api \
  --resource-group UniversityTuitionRG
```

### Enable Detailed Error Messages
```bash
az webapp config set \
  --name university-tuition-api \
  --resource-group UniversityTuitionRG \
  --detailed-error-logging-enabled true
```
