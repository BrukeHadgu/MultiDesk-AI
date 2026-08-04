# Decision 1: Clean Architecture (4-project structure)
## Decision
Split the backend into four projects:
- MultiDesk.Domain — entities, no dependencies
- MultiDesk.Application — business logic, interfaces, DTOs
- MultiDesk.Infrastructure — EF Core, PostgreSQL, repositories
- MultiDesk.Api — controllers, middleware, Program.cs
## Reason
- Dependencies point inward — Domain has no dependencies on anything
- Can unit test Application layer without database
- Can swap PostgreSQL for another database by only changing Infrastructure
- Same pattern learned and applied in TmsApi
- Matches industry standard for enterprise .NET applications