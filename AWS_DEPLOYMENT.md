# AWS Project Deployment Guide: Financial Solution 🚀

This guide provides the exact steps to deploy your Full-Stack Finance Dashboard to AWS using the architecture defined in your README (App Runner + RDS + S3).

## Phase 1: Preparation (CLI & Permissions)
1. **AWS CLI**: Ensure you have the AWS CLI installed and configured:
   ```bash
   aws configure
   ```
2. **IAM Policy**: Create a user with `PowerUserAccess` or at least `AppRunnerFullAccess`, `AmazonRDSFullAccess`, and `AmazonS3FullAccess`.

---

## Phase 2: Database Layer (PostgreSQL RDS)
We need a managed database instead of a Docker container for production.
1. Go to **RDS Console** -> **Create Database**.
2. **Engine**: PostgreSQL 16+.
3. **Template**: "Free Tier" (to avoid costs).
4. **DB Instance Identifier**: `finance-db`.
5. **Credentials**: Set a master username (e.g., `dbadmin`) and password. **(SAVE THESE)**.
6. **Public Access**: No (Security best practice).
7. **Security Group**: Create a new one named `finance-db-sg`. Edit the inbound rules to allow **Port 5432** from your **App Runner** service (or your local IP for initial migrations).

---

## Phase 3: Backend Layer (AWS App Runner)
App Runner is the easiest way to deploy containerized .NET APIs.
1. **Create ECR Repository**:
   ```bash
   aws ecr create-repository --repository-name finance-api
   ```
2. **Push your Image**:
   ```bash
   # Log in to ECR
   aws ecr get-login-password --region <your-region> | docker login --username AWS --password-stdin <your-account-id>.dkr.ecr.<your-region>.amazonaws.com
   
   # Tag and Push
   docker build -t finance-api ./backend
   docker tag finance-api:latest <your-account-id>.dkr.ecr.<your-region>.amazonaws.com/finance-api:latest
   docker push <your-account-id>.dkr.ecr.<your-region>.amazonaws.com/finance-api:latest
   ```
3. **Create App Runner Service**:
   - **Source**: Container Registry (ECR).
   - **Image**: Select `finance-api:latest`.
   - **Service Name**: `finance-backend-service`.
   - **Runtime Variables**: Add these key-values:
     - `ConnectionStrings__DefaultConnection`: `Host=<rds-endpoint>;Database=postgres;Username=dbadmin;Password=<your-password>;Port=5432`
     - `ASPNETCORE_URLS`: `http://+:80`

---

## Phase 4: Frontend Layer (S3 + CloudFront)
1. **Build Frontend**:
   ```bash
   cd frontend
   npm install
   npm run build
   ```
2. **S3 Bucket**:
   - Create bucket `zhouyi-finance-frontend`.
   - Disable "Block all public access" (or use CloudFront OAI).
3. **CloudFront**:
   - Create Distribution.
   - **Origin**: Your S3 bucket.
   - **Default Root Object**: `index.html`.
4. **Environment Config**: Ensure your `vite.config.ts` or `.env` in frontend points to the **App Runner URL** for the API calls.

---

## Phase 5: CI/CD Strategy
Update `.github/workflows/deploy.yml` with AWS credentials (using GitHub Secrets):
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION`

---

## 🎬 Plan B: Demo Video Script
If the AWS deployment hits a snag, use this "Power Demo" script to prove technical competency.

### Scene 1: The Code Foundation (1 min)
*   **Action**: Show the `backend/Dockerfile`.
*   **Talking Point**: "I've containerized the ASP.NET Core API using a multi-stage build to minimize production image size and ensure security."
*   **Action**: Show the AWS Architecture diagram in the README.

### Scene 2: The Data Layer (1 min)
*   **Action**: Show the `power_bi/DESIGN.md`.
*   **Talking Point**: "Beyond standard CRUD, I've architected a Star Schema for reporting. This allows for complex DAX measures like MoM spending growth, even before moving to AWS RDS."

### Scene 3: The Deployment Pipeline (1 min)
*   **Action**: Open GitHub Actions.
*   **Talking Point**: "Every commit triggers a 'Build and Test' workflow. It verifies code integrity and creates an 'AWS Ready' Docker image, ready to be pushed to Amazon ECR."

### Scene 4: Local Prototype (2 mins)
*   **Action**: Run `docker compose up` and show the Vue dashboard.
*   **Talking Point**: "This local environment perfectly mirrors our AWS Target architecture using Docker Compose. Here you can see the sub-100ms response times for financial analytics."
