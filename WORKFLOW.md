# System Architecture & Workflow

This document outlines the core technical workflows, architectural patterns, and security layers of the Financial Solution project.

---

## 🛡️ Backend Middleware Pipeline (The Assembly Line)
Every request from the Frontend passes through these stations in order. If a check fails, the request "short-circuits" and returns an error immediately.

1.  **Swagger (`UseSwagger`)**: Generates API documentation (Development only).
2.  **HTTPS Redirection (`UseHttpsRedirection`)**: Enforces encrypted communication.
3.  **CORS (`UseCors`)**: Restricts access to trusted origins (e.g., `localhost:5173`). Must be before Auth to handle browser pre-flight `OPTIONS` requests.
4.  **Authentication (`UseAuthentication`)**: Decodes the JWT token to identify the user. Populates `User.Identity`.
5.  **Authorization (`UseAuthorization`)**: Checks if the identified user is permitted to access the requested resource (e.g., `[Authorize]` attribute).
6.  **Routing / Controllers**: Directs the request to the specific Action logic.

---

## 🕹️ Controllers and Actions (The Functional Entry Points)

| Controller | Primary Actions | Responsibility |
| :--- | :--- | :--- |
| **Analytics** | `GetTrends`, `GetMonthlySummary` | Data aggregation, chart preparation, and KPI calculation. |
| **Transactions** | `Get`, `Create`, `Update`, `ImportPdf` | CRUD operations and AI-driven bank statement parsing. |
| **Auth** | `Login`, `Register` | User lifecycle and JWT token issuance. |
| **BudgetSpaces** | `GetList`, `Create` | Managing multi-tenant workspace environments. |
| **Categories/Tags** | `GetList`, `Create` | Taxonomy and organizational metadata management. |

---

## 🚀 Data & Performance Workflow

### 1. The Caching Layer (Redis)
To optimize performance, Redis is used as a **Layer** between the API and the Database.
*   **Cache Hit**: Data is returned from RAM (~1-2ms).
*   **Cache Miss**: Data is fetched from Postgres, calculated, then saved to Redis for future hits (~100-200ms).

### 2. Timezone Integrity (Noon UTC Pattern)
To prevent "off-by-one-day" errors across different client timezones, all transaction dates are normalized to **Noon UTC (12:00:00Z)** before persistence. This ensures the date portion remains consistent globally.

### 3. LINQ Optimization
Queries use `IQueryable` to push filtering (`Where`), sorting (`OrderBy`), and pagination (`Skip/Take`) to the **PostgreSQL Engine** level. This avoids fetching large datasets into application memory.

---

## 🎨 Frontend Workflow (Vue.js 3)
*   **State Management**: Pinia stores handle Auth status and Token persistence.
*   **Optimistic UI**: The interface reflects changes immediately while background Axios requests confirm persistence.
*   **Reactivity**: Uses `computed` properties for real-time calculations (e.g., `netIncomeTrend = income - expense`).
