# Personal Finance Dashboard 💰

A full-stack finance tracking application built with **Vue 3** and **ASP.NET Core**.

## Tech Stack

- **Frontend:** Vue 3 (Composition API), Pinia, Vue Router, TailwindCSS, Vite
- **Backend:** ASP.NET Core 8.0/10.0 Web API, Entity Framework Core, JWT Auth
- **Database:** PostgreSQL (via Docker)

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

## Features implemented

- **Authentication:** Register/Login with JWT.
- **Budget Spaces:** Isolate your finances.
- **Transactions:** Create, edit, delete, and filter income/expenses.
- **Analytics:** Monthly summaries and category breakdowns.
- **Tags & Categories:** Manage your metadata.

## Troubleshooting

- **Database Connection Refused:** Ensure Docker is running. If port 5433 is blocked, check `docker-compose.yml` and `appsettings.json`.
- **CORS Errors:** The backend is configured to allow `http://localhost:5173`. If your frontend runs on a different port, update `Program.cs`.
