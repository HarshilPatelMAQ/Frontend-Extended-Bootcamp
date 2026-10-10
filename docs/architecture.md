# Architecture & Technical Documentation

## Architectural Overview

The **Smart Service Request Management Portal (SSRMP)** is built adhering to **Clean Architecture** (Onion/Hexagonal Architecture) principles. The codebase is organized into four decoupled projects to isolate business rules from infrastructure and presentation concerns:

```
src/
├── ServicePortal.Domain/          # Core Domain Entities, Enums, Interfaces, Value Objects
├── ServicePortal.Application/     # Use Cases, Business Logic, SLA Engines, Workflow Rules
├── ServicePortal.Infrastructure/  # EF Core DbContext, SQL Server Migrations, Identity Setup
└── ServicePortal.Web/             # ASP.NET Core MVC Controllers, Razor Views, SignalR, Background Services
tests/
├── ServicePortal.UnitTests/       # xUnit Unit Test Suite for Domain & Service Logic
└── ServicePortal.IntegrationTests/# Integration Tests for Database & End-to-End Workflows
```

---

## Security & Authorization Design

1. **Role-Based Access Control (RBAC)**:
   - `Administrator`: Full access to configure support teams, SLA policies, manage users, and view audit logs.
   - `TeamLead`: Manage team queues, assign tickets to agents, monitor SLA breaches, and reassign requests.
   - `SupportAgent`: Work on assigned tickets, update status, add internal notes/public responses, and resolve requests.
   - `Requester`: Submit service requests, track request history, and communicate with support teams.

2. **Insecure Direct Object Reference (IDOR) Prevention**:
   - Every request lookup checks `RequesterId == currentUserId` for non-staff users on the server side in `ServiceRequestsController.cs` before returning request data or allowing status modifications.

3. **Concurrency Control**:
   - `ServiceRequest` entity includes a `RowVersion` `[Timestamp]` byte array to enforce optimistic concurrency control in EF Core and prevent silent overwrites.

4. **CSRF & XSS Protection**:
   - All HTTP POST actions enforce `[ValidateAntiForgeryToken]`.
   - Razor automatic HTML output encoding prevents XSS script execution.

---

## SLA Calculation & Background Processing

1. **SLA Business Hours Engine (`ISlaCalculationService`)**:
   - Calculates target due dates based on business hours (Mon-Fri, 09:00 - 18:00).
   - Skips weekend hours automatically when computing `ResponseDueDate` and `ResolutionDueDate`.

2. **Automated Background Services (`IHostedService`)**:
   - **`SlaBreachCheckerService`**: Periodically scans active tickets for upcoming SLA deadlines and flags breached requests automatically, generating audit records and notifications.
   - **`AutoCloseResolvedRequestsService`**: Scans resolved requests older than the 48-hour grace period and automatically transitions them to `Closed`.
