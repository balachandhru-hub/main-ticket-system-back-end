🎫 Main Ticket System – Back-End
This is the .NET 8 Web API back-end for the internal Ticket System used by our organization. It supports all ticket lifecycle operations, scheduled background tasks, and integrates with our internal micro frontends and systems.

🔧 Tech Stack
.NET 8.0 Web API

Entity Framework Core

SQL Server

Quartz.NET (job scheduling)

AutoMapper

JWT Authentication

Swagger (for internal testing)

⚙️ Setup Instructions
Prerequisites
.NET 8 SDK

SQL Server (or configured equivalent)

Access to internal Git repository

# Clone the repo
git clone [https://<your-internal-git>/main-ticket-system-back-end.git](https://github.com/Chervic-Advisory-Services/main-ticket-system-back-end.git)
cd main-ticket-system-back-end
cd TicketSystemAPI

# Restore dependencies
dotnet restore

# Run the API
dotnet run

🗂 Modules Overview
Ticket Management: Create, update, assign, close tickets

User Access Control: Role-based access (e.g., Intern, Support, Admin)

Quartz Jobs: Scheduled tasks (e.g., auto-close stale tickets)

Audit Logging: (if enabled)

Integration Hooks: For front-end MFEs or other internal services
