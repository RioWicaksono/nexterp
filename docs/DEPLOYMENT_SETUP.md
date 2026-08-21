# Deployment Setup Guide - NEXTERP ERP

## Vercel (Frontend - Next.js)

### Step 1: Install Vercel GitHub App
1. Buka https://github.com/settings/connections/applications — OR
2. Buka https://vercel.com → Account Settings → Git Connections
3. Install/connect Vercel ke repo `RioWicaksono/nexterp`

### Step 2: Get Vercel Credentials
1. Buka https://vercel.com/dashboard
2. Buka NEXTERP project → Settings → General
3. Copy **Organization ID** dan **Project ID**
4. Buat token: Settings → Tokens → Create (scope: Full Account)

### Step 3: Add GitHub Secrets
Repository: https://github.com/RioWicaksono/nexterp/settings/secrets/actions
Add these secrets:
- `VERCEL_TOKEN` = your-vercel-token
- `VERCEL_ORG_ID` = org_xxxxxxxxxxxxx
- `VERCEL_PROJECT_ID` = prj_xxxxxxxxxxxxx
- `NEXT_PUBLIC_API_URL` = https://your-railway-url.up.railway.app

### Step 4: Fix Branch Name
Edit `.github/workflows/deploy.yml` — change `main` to `master`:
```yaml
on:
  push:
    branches: [master]  # was [main]
```

---

## Railway (Backend - .NET API)

### Step 1: Connect Railway to GitHub
1. Buka https://railway.app
2. Buka NEXTERP project → Settings → GitHub
3. Connect repo `RioWicaksono/nexterp`
4. Set **Root Directory** ke `/` (repo root)

### Step 2: Configure Railway Variables
In Railway project Settings → Variables, add:
- `DATABASE_URL` = your-postgres-connection-string
- `Jwt:SecretKey` = your-secure-32-char-minimum-secret
- `Jwt:AccessTokenExpirationMinutes` = 15
- `Jwt:RefreshTokenExpirationDays` = 7
- `ASPNETCORE_ENVIRONMENT` = Production
- `PORT` = 8080 (Railway default)
- `REDIS_URL` = your-redis-connection-string

### Step 3: Railway.json Config (Already Done)
Current `railway.json` in root:
```json
{
  "$schema": "https://railway.app/schema.json",
  "build": { "builder": "DOCKERFILE" },
  "deploy": { "numReplicas": 1 }
}
```
✅ This tells Railway to use the Dockerfile in repo root.

### Step 4: Dockerfile Path
Since railway.json is in repo root and `builder: "DOCKERFILE"` is set,
Railway will use `Dockerfile` in root. ✅ Already correct.

---

## Trigger Deployment After Setup

### Vercel (via GitHub Actions):
```bash
git push  # triggers deploy.yml → Vercel auto-deploys
```

### Railway:
- Auto-deploys on push to `master` if GitHub connected
- Or manual: Railway dashboard → Deploy → Redeploy

---

## Verify Deployment URLs

After setup, update `nextjs-frontend/.env.local`:
```env
NEXT_PUBLIC_API_URL=https://your-railway-app.railway.app
```
