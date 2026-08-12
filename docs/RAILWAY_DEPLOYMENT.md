# Railway Deployment Guide

## Prerequisites
- Railway CLI installed (`npm i -g @railway/cli`)
- Railway account (free tier available at railway.app)
- Git repository with your code

---

## Option 1: Deploy via Railway CLI (Recommended)

### Step 1: Login
```bash
railway login
```

### Step 2: Initialize Project
```bash
cd "d:/RW/PROJECT RW/ERP"
railway init
```
Select:
- Project name: `nexterp`
- Environment: `Production`

### Step 3: Provision Database & Cache

**PostgreSQL Database:**
```bash
railway add postgresql
```

**Redis Cache:**
```bash
railway add redis
```

### Step 4: Deploy Backend API

```bash
cd ERP.API
railway up
```

Set environment variables in Railway dashboard:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
JWT_SECRETKEY=YourSuperSecretKeyAtLeast32Characters!
```

### Step 5: Deploy Frontend

```bash
cd ERP.WebUI
railway up
```

Set environment variables:
```
NEXT_PUBLIC_API_URL=https://your-api.railway.app/api
NEXT_PUBLIC_APP_URL=https://your-app.railway.app
```

---

## Option 2: Deploy via GitHub (Automated)

### Step 1: Push Code to GitHub
```bash
git init
git add .
git commit -m "feat: NEXTERP ERP System - Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/nexterp.git
git push -u origin main
```

### Step 2: Connect to Railway
1. Go to https://railway.app
2. Click "New Project" → "Deploy from GitHub repo"
3. Select your repository
4. Railway auto-detects `.NET` and `Next.js`

### Step 3: Configure Services

**Backend API:**
1. Root directory: `ERP.API`
2. Build command: `dotnet build -c Release`
3. Start command: `dotnet run -c Release`
4. Add PostgreSQL and Redis plugins

**Frontend:**
1. Root directory: `ERP.WebUI`
2. Build command: `npm run build`
3. Start command: `npm start`
4. Add environment variables

---

## Database Migration on Railway

After deployment, run migrations:

```bash
railway run dotnet ef database update --project ERP.Infrastructure --startup-project ERP.API
```

Or create initial migration:
```bash
railway run dotnet ef migrations add InitialCreate --project ERP.Infrastructure --startup-project ERP.API
```

---

## Environment Variables Reference

### Backend (ERP.API)
| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ASPNETCORE_URLS` | Listen URL | `http://0.0.0.0:8080` |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | Auto from plugin |
| `Jwt__SecretKey` | JWT signing key (min 32 chars) | `YourSecretKeyHere123456789012345` |
| `Jwt__Issuer` | Token issuer | `ERP.System` |
| `Jwt__Audience` | Token audience | `ERP.Client` |

### Frontend (ERP.WebUI)
| Variable | Description | Example |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | Backend API URL | `https://api-xxx.railway.app/api` |
| `NODE_ENV` | Environment | `production` |

---

## Troubleshooting

### Build Failures
```bash
# Check build logs
railway logs --deployment

# Redeploy
railway up --detach
```

### Database Connection
1. Verify PostgreSQL plugin is attached
2. Check connection string format
3. Ensure SSL is configured if required

### CORS Issues
Update CORS configuration in `ERP.API/Program.cs`:
```csharp
app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
```

Or specify your Railway frontend URL:
```csharp
.WithOrigins("https://your-frontend.railway.app")
```

---

## Health Check

After deployment, verify:
- API: `https://your-api.railway.app/health`
- Swagger: `https://your-api.railway.app/swagger`

---

## Scaling

Railway free tier:
- 500 hours/month
- 1GB RAM per service
- Shared CPU

For production, consider upgrading to paid plan for:
- Private networking
- Multiple replicas
- Custom domains with SSL
