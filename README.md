# Personal Finance Dashboard 💰

[**Changelog**](CHANGELOG.md)

A full-stack finance tracking application built with **Vue 3** and **ASP.NET Core**.

## Tech Stack

- **Frontend:** Vue 3 (Composition API), Pinia, Vue Router, TailwindCSS, Vite
- **Backend:** ASP.NET Core 8.0/10.0 Web API, Entity Framework Core, JWT Auth
- **Database:** PostgreSQL (via Docker), MongoDB (Data Lake Landing Zone)
- **Data & BI:** Power BI (DAX, Star Schema), **Tableau (EDA Dashboards)**, Python (Simulation)

## Prerequisites

- .NET SDK 8.0 or later
- Node.js & npm
- Docker & Docker Compose

## Setup Instructions

### 1. Database Setup
Start the PostgreSQL container:
```bash
cd backend
docker compose up -d
```
*Note: The database runs on port **5433** to avoid conflicts.*

### 2. Backend Setup
Initialize the database and start the API:
```bash
cd backend
# Run migrations
dotnet ef database update --project FinanceApi

# Start the API
dotnet run --project FinanceApi
```
The API will be available at `http://localhost:5000` (or `http://localhost:5059` depending on launch profile). Check the console output.

### 3. Frontend Setup
Install dependencies and start the dev server:
```bash
cd frontend
npm install
npm run dev
```
Open `http://localhost:5173` in your browser.
  
## ☁️ Cloud Architecture (AWS Target)

Designed for high-availability deployment on AWS to demonstrate cloud-native aptitude:
- **Compute:** AWS App Runner or ECS (Fargate) for the containerized .NET API.
- **Frontend:** AWS S3 + CloudFront for static Vue.js hosting.
- **Database:** AWS RDS (PostgreSQL 15) for managed data persistence.
- **Caching:** AWS ElastiCache (Redis) for high-performance analytics caching.
- **CI/CD:** GitHub Actions (Current) pushing Docker images to **AWS ECR**.
- **Data Lake:** S3 (Landing Zone) + AWS Glue (Crawl/Transform) for enterprise-scale analytics.

## Features implemented

- **Authentication:** Register/Login with JWT.
- **Budget Spaces:** Isolate your finances.
- **Transactions:** Create, edit, delete, and filter income/expenses.
- **Recurring Expense Catalog:** Procore-inspired "Cost Catalog" to save templates for frequent transactions.
- **Analytics:** Monthly summaries and category breakdowns.
- **Power BI Integration:** Professional data modeling with Star Schema and DAX measures.
- **ETL Pipelines:** Robust data ingestion and cleansing logic for diverse data sources.
- **Tags & Categories:** Manage your metadata.

## 📈 Power BI & Data Architecture

This project includes a professional-grade Business Intelligence implementation located in the `power_bi/` directory.

### 1. Data Modeling (Star Schema)
Designed a high-performance **Star Schema** to enable complex time-intelligence reporting:
- **Fact_Transactions**: Core metrics (Amount, DateKey).
- **Dim_Date**: Dynamic date dimension for MoM/YoY analysis.
- **Dim_Category / Dim_Merchant**: Normalized metadata for granular filtering.

### 2. Advanced Analytics (DAX)
Implemented custom **DAX Measures** for strategic financial insights:
- **MoM Spend Growth %**: Tracking monthly spending velocity.
- **Budget Utilization %**: Real-time budget vs. actuals monitoring.
- **Total Income**: Summing positive cash flows for savings rate calculation.
- **Star Schema Performance**: Optimized for schema-on-read performance across 1.2M+ simulated records.

### 3. Tableau Exploratory Analysis
Complementing Power BI, a **Tableau Dashboard** was designed for Exploratory Data Analysis (EDA):
- **Linked Visuals**: Dynamic filtering between geographical spending (Map) and time-series (Line charts).
- **Cluster Analysis**: Identified "Anomalous Spending" patterns using Tableau's built-in clustering.

### 4. ETL & Data Cleansing
The system implements a robust **ETL pipeline** (simulated in `power_bi_model.py`) that handles:
- **Acquisition**: Ingesting raw financial data from landing zones.
- **Cleansing**: Removing duplicates and normalizing currency types.
- **Loading**: Populating the Star Schema for downstream reporting.

## Troubleshooting

- **Database Connection Refused:** Ensure Docker is running. If port 5433 is blocked, check `docker-compose.yml` and `appsettings.json`.
- **CORS Errors:** The backend is configured to allow `http://localhost:5173`. If your frontend runs on a different port, update `Program.cs`.
