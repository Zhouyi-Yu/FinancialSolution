# Changelog

All notable changes to this project will be documented in this file.

## [1.4.0] - 2026-01-15

### Added
- **Recurring Expense Catalog**: Procore-inspired "Cost Catalog" feature allowing users to save transactions as templates.
- **Workflow Automation**: One-click "Apply" to auto-fill transaction forms from the catalog, reducing repetitive data entry.
- **Catalog Hint System**: Interactive walkthrough component within the transaction form for first-time users.

### Fixed
- **Navigation Inconsistency**: Resolved critical routing bug where `/dashboard` redirect was missing, causing blank screens on "Back" navigation.
- **Backend Serialization**: Fixed `500 Internal Server Error` in `TransactionTemplates` caused by JSON circular references and excessive EF Core Includes.
- **Auth Reliability**: Corrected backend `OwnerUserId` property reference mismatches in template management endpoints.
- **Category Synchronization**: Ensured categories are correctly fetched and displayed within the transaction dialog alongside catalog items.

## [1.3.0] - 2026-01-13

### Added
- **Transaction Management**: Users can now manually Add, Edit, and Delete transactions via a new modal interface.
- **Interactive Trends**: "Cash Flow" and "Expense" charts now support time-range filtering (1D, 1W, 1M, 3M, 6M, Max) with automatic data granularity adjustment (Hourly/Daily/Monthly).
- **PDF Upload Feature**: Implemented bank statement parsing with duplicate detection and preview confirmation.
- **Debug Console**: Added an in-app debug overlay for real-time frontend logging.

### Fixed
- **Chart Reliability**: Resolved backend 500 errors caused by Npgsql/EF Core timezone mismatches (`DateTimeKind.Utc`).
- **Authentication**: Fixed session persistence issue causing 401 errors on page refresh; added auto-redirect to login on session expiry.
- **UI Glitches**: Removed persistent mock data sections ("Department Spend") and fixed Vue template malformation bugs.

## [1.2.0] - 2026-01-11

### Added
- **Power BI Implementation**: Added full Star Schema design and DAX measures for financial analytics.
- **Data Lake Architecture**: Documented MongoDB landing zone and ETL strategy for enterprise data handling.
- **Python Analytics Simulation**: Added `power_bi_model.py` to verify data modeling and analytics logic in non-Windows environments.
- **ETL Pipelines**: Implemented structured data acquisition and cleansing logic.

## [1.1.0] - 2026-01-07

### Added
- **AWS Target Architecture**: Documented the cloud-native migration plan (App Runner, RDS, ElastiCache).
- **Dockerization**: Added a multi-stage `Dockerfile` and updated `docker-compose.yml` for unified local development.
- **Redis Integration**: Added Redis service to the stack for high-performance caching (Procore-aligned).
- **CI/CD Enhancements**: Integrated Docker build verification into GitHub Actions.
- **SRS Documentation**: Added comprehensive Software Requirements Specification.

## [1.0.0] - 2026-01-07

### Added
- **Initial Release**: Core Personal Finance Dashboard functionality.
- **Backend**: ASP.NET Core API with PostgreSQL and JWT Authentication.
- **Frontend**: Vue 3 SPA with Pinia and TailwindCSS.
- **CI/CD**: Initial GitHub Actions for .NET build and test.
- **Testing**: Service-level unit tests for backend logic.
