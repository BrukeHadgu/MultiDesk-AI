# MultiDesk AI — University Student Support Portal
A full-stack helpdesk system for universities with AI-powered reply suggestions.

## Tech Stack
| Backend | C# 12, ASP.NET Core 10, Entity Framework Core 10 |
| Database | PostgreSQL | pgadmin 4
| Frontend | Angular 22, TypeScript, SCSS |
| Auth | ASP.NET Core Identity, JWT 
| AI | OpenAI gpt-4o-mini (reply suggestions) 
| Architecture | Clean Architecture (Domain, Application, Infrastructure, Api) 

## Project Structure
MultiDesk-AI/
MultiDesk.slnx
MultiDesk.Domain/   Entities, value objects (no dependencies)
MultiDesk.Application/    Business logic, DTOs, interfaces, MediatR
MultiDesk.Infrastructure/   EF Core, PostgreSQL, repositories
MultiDesk.Api/    Controllers, middleware, Program.cs
MultiDesk-Client/   Angular 22 frontend (later)
docs/   Architecture decisions, API docs