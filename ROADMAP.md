# Project Roadmap

## Overview

This document is the implementation roadmap for the **LMS Design Patterns PoC** — a small ASP.NET Core 8 Web API that demonstrates practical application of three design patterns from the first six chapters of *Head First Design Patterns*.

The project is a **two-day PoC**. The goal is a running application that makes the right pattern choices for the right reasons — not a production system, and not a pattern checklist.

---

## Selected Patterns

| Pattern | Feature | Why it fits |
|---|---|---|
| Strategy | Grading algorithms | Each assignment type requires different grading logic; the workflow stays the same |
| Factory | Strategy resolution | One place to decide which strategy to create; eliminates scattered `new` calls |
| Observer | Post-grading events | Multiple independent services react to the same event without coupling to the grader |

The remaining patterns from chapters 1–6 (Decorator, Singleton, Command) are documented in `docs/` and not implemented. Each implementation decision is driven by the business domain, not by the desire to use a pattern.

---

## Architecture Decision Record

**Why these three patterns?**

The core workflow — create assignment → submit → grade → react — naturally surfaces all three. Grading has algorithm variation (Strategy). Something must choose the right algorithm (Factory). Grading completion triggers independent downstream actions (Observer).

**Why this domain?**

An LMS provides a single linear business workflow with enough variation in each step to make the patterns meaningful. The domain is familiar enough that the reader spends zero time understanding the business and all their time understanding the patterns.

**What was explicitly excluded?**

Authentication, persistence (real database), assignment creation UI, student management, and any pattern not justified by the domain. In-memory repositories are used throughout so there is no infrastructure noise.

**Observer mechanism choice**

A simple hand-rolled in-process event bus is used instead of MediatR. This is a deliberate choice: MediatR would delegate the Observer pattern to a library, hiding the mechanism. Writing the event bus directly demonstrates the pattern itself.

**Factory vs Abstract Factory**

The implementation uses a **Simple Factory** (a class with a method that resolves the right strategy by type). This is not the GoF Factory Method pattern, which uses inheritance and subclass responsibility. The `docs/04-Factory.md` file explains this distinction explicitly.

---

## Project Structure

```
LMS.sln
├── LMS.Domain/
│   ├── Entities/         Student, Assignment, Submission, Grade
│   ├── Enums/            AssignmentType, SubmissionStatus
│   └── Interfaces/       IGradingStrategy, IEventBus, ISubscriber
│                         IAssignmentRepository, ISubmissionRepository
├── LMS.Application/
│   ├── Grading/          QuizGradingStrategy, ProgrammingGradingStrategy, EssayGradingStrategy
│   ├── Factory/          GradingStrategyFactory
│   ├── Events/           AssignmentGradedEvent, InProcessEventBus
│   ├── Subscribers/      NotificationSubscriber, ProgressSubscriber, AuditSubscriber
│   └── Services/         GradingService, SubmissionService
├── LMS.Infrastructure/
│   └── Repositories/     InMemoryAssignmentRepository, InMemorySubmissionRepository
└── LMS.API/
    ├── Controllers/       AssignmentsController, SubmissionsController
    └── Program.cs
docs/
└── notes.md
```

---

## Milestone 1 — Project Setup

**Goal**: A clean, runnable solution with nothing inside it yet.

**Tasks**

- Create the solution with four projects: `LMS.Domain`, `LMS.Application`, `LMS.Infrastructure`, `LMS.API`
- Configure Dependency Injection in `Program.cs`
- Configure Swagger/OpenAPI
- Add basic structured logging (built-in `ILogger<T>`)
- Add a health-check endpoint to confirm the app runs
- Create the initial Git commit

**Deliverable**: `GET /health` returns 200. Swagger UI loads.

---

## Milestone 2 — Domain Modeling

**Goal**: The core entities and repository abstractions are in place. No business logic yet.

**Entities**

- `Student` — Id, Name
- `Assignment` — Id, Title, AssignmentType, MaxScore
- `Submission` — Id, StudentId, AssignmentId, Answer, Status, SubmittedAt
- `Grade` — Id, SubmissionId, Score, Feedback, GradedAt

**Enums**

- `AssignmentType` — Quiz, ProgrammingAssignment, Essay
- `SubmissionStatus` — Pending, Graded

**Repository interfaces** (in `LMS.Domain`)

- `IAssignmentRepository` — GetById, Add, GetAll
- `ISubmissionRepository` — GetById, Add, Update, GetByAssignment

**In-memory implementations** (in `LMS.Infrastructure`)

- `InMemoryAssignmentRepository`
- `InMemorySubmissionRepository`

Register both as singletons in DI so data persists for the duration of a session.

**Deliverable**: Domain layer is complete. Repositories compile and are registered in DI.

> No service layer, no controllers yet — just the foundation.

---

## Milestone 3 — Strategy Pattern

**Goal**: Different assignment types grade differently. The grading workflow itself stays unchanged.

**Business problem**

A Quiz grades by comparing the submitted answer against a key and returns a numeric score. A Programming Assignment grades by checking test case results. An Essay grades by keyword analysis and length. These are three different algorithms applied through one interface.

**Interface** (in `LMS.Domain`)

```csharp
public interface IGradingStrategy
{
    GradeResult Grade(Submission submission, Assignment assignment);
}
```

**Implementations** (in `LMS.Application/Grading/`)

- `QuizGradingStrategy` — exact match against answer key, returns 0 or full score
- `ProgrammingGradingStrategy` — checks for required keywords in the answer (simulates test cases), partial score per keyword found
- `EssayGradingStrategy` — scores by word count meeting a minimum threshold, then keyword bonus

Keep each implementation simple and obviously different — the goal is to show the algorithm swap, not to write a real grader.

**Deliverable**: All three strategies implement `IGradingStrategy` and produce distinct results for the same input. Unit-testable in isolation.

---

## Milestone 4 — Factory Pattern

**Goal**: One place decides which strategy to instantiate. No strategy `new` calls anywhere else.

**Business problem**

`GradingService` needs a strategy before it can grade. It should not know how to construct strategies — that is a separate responsibility.

**Interface** (in `LMS.Domain`)

```csharp
public interface IGradingStrategyFactory
{
    IGradingStrategy Create(AssignmentType type);
}
```

**Implementation** (in `LMS.Application/Factory/`)

```csharp
public class GradingStrategyFactory : IGradingStrategyFactory
{
    public IGradingStrategy Create(AssignmentType type) => type switch
    {
        AssignmentType.Quiz                => new QuizGradingStrategy(),
        AssignmentType.ProgrammingAssignment => new ProgrammingGradingStrategy(),
        AssignmentType.Essay               => new EssayGradingStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
```

Register `IGradingStrategyFactory` → `GradingStrategyFactory` as a singleton in DI.

> Note: This is a Simple Factory, not GoF Factory Method. The distinction is documented in `docs/04-Factory.md`. The Simple Factory is the right fit here — Factory Method would require subclassing `GradingService`, which adds complexity without solving a real problem in this domain.

**Deliverable**: `GradingService` depends only on `IGradingStrategyFactory`. No direct strategy instantiation outside the factory.

---

## Milestone 5 — Observer Pattern

**Goal**: Once grading completes, independent services react without the grader knowing about them.

**Business problem**

After a submission is graded, three things need to happen:
- The student receives a notification with their score
- The student's progress record is updated
- An audit entry is written

These are independent concerns. Coupling them directly to `GradingService` means every new post-grading requirement modifies the grader — violating the Open/Closed Principle.

**Event** (in `LMS.Application/Events/`)

```csharp
public record AssignmentGradedEvent(
    Guid SubmissionId,
    Guid StudentId,
    Guid AssignmentId,
    double Score,
    string Feedback
);
```

**Event bus** (in `LMS.Application/Events/`)

A hand-rolled in-process event bus — intentionally simple to keep the Observer mechanism visible.

```csharp
public interface IEventBus
{
    void Subscribe<T>(ISubscriber<T> subscriber);
    void Publish<T>(T @event);
}
```

```csharp
public class InProcessEventBus : IEventBus
{
    private readonly Dictionary<Type, List<object>> _subscribers = new();

    public void Subscribe<T>(ISubscriber<T> subscriber) { ... }
    public void Publish<T>(T @event) { ... }
}
```

**Subscribers** (in `LMS.Application/Subscribers/`)

- `NotificationSubscriber` — logs a notification message (simulates sending an email or push notification)
- `ProgressSubscriber` — updates an in-memory student progress record
- `AuditSubscriber` — writes an audit log entry via `ILogger`

**Wiring** in `Program.cs`

```csharp
var bus = app.Services.GetRequiredService<IEventBus>();
bus.Subscribe(app.Services.GetRequiredService<NotificationSubscriber>());
bus.Subscribe(app.Services.GetRequiredService<ProgressSubscriber>());
bus.Subscribe(app.Services.GetRequiredService<AuditSubscriber>());
```

**Deliverable**: Grading a submission publishes the event. All three subscribers react. `GradingService` has no reference to any subscriber.

---

## Milestone 6 — API Endpoints

**Goal**: The full business workflow is executable via Swagger. No Postman collection needed.

**Endpoints**

| Method | Route | Description |
|---|---|---|
| POST | `/assignments` | Create an assignment with a type and answer key |
| POST | `/submissions` | Submit an answer for an assignment |
| POST | `/submissions/{id}/grade` | Grade a submission (triggers the full Strategy → Factory → Observer chain) |
| GET | `/assignments/{id}` | Retrieve an assignment |
| GET | `/submissions/{id}` | Retrieve a submission with its grade |

**Error handling**

Return `ProblemDetails` (RFC 7807) consistently. Use `Results.Problem(...)` for 404, 400, and 422 cases. Do not swallow exceptions silently.

**Swagger documentation**

Add XML doc comments to all controllers. Enable `IncludeXmlComments` in Swagger config. Each endpoint should have a one-line summary describing what it does in the workflow.

**Deliverable**: The complete assignment → submission → grading workflow runs end-to-end in Swagger UI with visible responses at each step.

---

## Day-by-Day Plan

| Day | Milestones | Expected output |
|---|---|---|
| Day 1 | M1 → M2 → M3 → M4 → M5 | All three patterns implemented and wired |
| Day 2 | M6 → M7 → M8 | Full API, documentation, clean repository |

M1 and M2 are the shortest milestones. If setup is fast, M3–M5 can be completed comfortably in Day 1. M6 is the most time-sensitive on Day 2 — complete the endpoints before documentation.

---

## Success Criteria

The project is complete when:

- The application runs with `dotnet run`
- The full assignment → submission → grading → event chain works end-to-end in Swagger
- Strategy, Factory, and Observer are correctly implemented and clearly separated
- The Factory vs Simple Factory distinction is documented
- The Observer mechanism is hand-rolled, not delegated to a library
- The `docs/` folder covers all six chapters
- A reviewer can identify every pattern, find the relevant files, and understand the design decision — without asking a question
