# MovieSpot (LDS Project)

Full-stack cinema management and booking platform developed as an academic project for Software Development Labs (LDS).

The project follows a multi-layer architecture and focuses strongly on automated testing, CI pipelines and reproducible environments.
While the backend is fully implemented, the frontend focuses on two critical features developed during the course.

------------------------------------------------------------

Project Purpose
This project was developed to apply and consolidate concepts related to:
- layered software architecture
- automated testing at different levels
- continuous integration
- containerized development environments

Special emphasis was placed on software quality and validation, rather than feature completeness.

------------------------------------------------------------

Features

Backend (Complete)
- Cinema, room and session management
- Booking and seat allocation
- Voucher and pricing logic
- RESTful API with clear separation of concerns
- Full business logic validation

Frontend (Partial)
- Implementation of two critical user-facing features
- Integration with backend API
- End-to-end system tests using Cypress

Mobile
- Android client developed in Kotlin (Jetpack Compose)
- CI lint validation

------------------------------------------------------------

Project Structure

Backend/          .NET API + unit & integration tests + Docker
Frontend/         Angular application + Cypress system tests
Android-Client/   Android (Kotlin / Jetpack Compose)
.gitlab-ci.yml    CI pipeline configuration

------------------------------------------------------------

Testing Strategy

Backend
- Unit tests
- Integration tests (API + database)

Frontend
- System / End-to-End tests using Cypress

Mobile
- Static analysis and lint checks in CI
- Support for local unit and instrumentation tests

------------------------------------------------------------

Docker Support

The backend includes Docker Compose configuration with:
- API container
- PostgreSQL database
- Separate database for integration tests

------------------------------------------------------------

CI/CD Pipeline

The repository includes a GitLab CI pipeline with:
- Lint
- Build
- Backend unit tests
- Backend integration tests
- Frontend system tests (Cypress)
- Android lint validation

------------------------------------------------------------

Technologies

Backend
- .NET 9
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Docker / Docker Compose

Frontend
- Angular
- TypeScript
- Cypress

Mobile
- Kotlin
- Jetpack Compose
- Gradle

------------------------------------------------------------

How to Run (Backend)

Using Docker:
cd Backend
docker compose up --build

Running locally:
cd Backend
dotnet restore
dotnet run

------------------------------------------------------------

Academic Context

Developed as part of the Software Development Labs (LDS) course, with focus on:
- clean architecture
- test-driven validation
- CI automation
- real-world development workflows

------------------------------------------------------------

Notes

This project prioritizes architecture, testing and development practices over feature completeness.
The backend is fully implemented, while the frontend reflects the scope and time constraints of the academic context.
