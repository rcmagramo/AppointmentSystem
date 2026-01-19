# Appointment Management System

## Overview
WPF desktop application with ASP.NET Core Web API backend for managing medical appointments.

## Prerequisites
- Visual Studio 2019+
- .NET Framework 4.8 SDK
- .NET 6.0/7.0 SDK

## How to Run
1. Open solution in Visual Studio
2. Set both projects to start (Solution Properties → Multiple Startup)
3. Press F5
4. API runs on https://localhost:7205
5. WPF client opens automatically

## Features
✓ CRUD operations for appointments
✓ MVVM with zero code-behind
✓ Dependency injection
✓ Entity Framework Core + SQLite
✓ Repository & Service patterns
✓ Error handling & logging
✓ Async operations
✓ Patient search functionality
✓ CSV export

## API Endpoints
- GET /api/appointments
- GET /api/appointments/{id}
- POST /api/appointments
- PUT /api/appointments/{id}
- DELETE /api/appointments/{id}

## Architecture
- **Single Responsibility**: Each class has one purpose
- **Dependency Inversion**: Interfaces + IoC
- **Separation of Concerns**: Controllers → Services → Repositories

## Future Enhancements
- Patient entity with full CRUD
- Appointment overlap validation
- Unit tests
- Authentication
