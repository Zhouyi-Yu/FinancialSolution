# Changelog

All notable changes to this project will be documented in this file.

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
