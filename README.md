# LMS Design Patterns Showcase

A learning-focused **ASP.NET Core 8 Web API** that demonstrates practical application of design patterns from the first six chapters of *Head First Design Patterns*.

The project follows a single business workflow — create assignment → submit → grade → react — and applies only the patterns that naturally solve real design problems within that workflow. Patterns are not forced into the application; they emerge from the code's own limitations.

---

## Goals

1. Identify the design problems that motivated the creation of design patterns.
2. Recognize when a pattern is appropriate and when it is not.
3. Apply the right pattern to a real business scenario.
4. Explain the naïve implementation, its limitations, and how the selected design pattern addresses those limitations.

This is **not** a pattern catalog. It is a problem-driven project where each pattern appears because it solves a specific, documented problem in the domain.

---

## Applied Patterns

| Pattern | Feature | Problem solved |
|---|---|---|
| Strategy | Grading algorithms | Each assignment type requires different logic; avoid large conditionals and make grading extensible |
| Factory | Strategy resolution | Centralize strategy creation; eliminate scattered `new` calls and reduce coupling |
| Observer | Post-grading events | Let independent services react to grading without coupling them to the grader |

The remaining patterns from chapters 1–6 — Decorator, Singleton, and Command — are documented in `docs/notes.md` with study notes and examples. They are not implemented because the domain does not present a problem they would naturally solve.

---

## Architecture Decisions

**Why these three patterns?**

The core workflow surfaces each pattern naturally. Grading varies by assignment type (Strategy). Something must select the right algorithm (Factory). Grading completion triggers independent downstream reactions (Observer). Each pattern solves a distinct problem; none is added for demonstration value alone.

**Why in-memory repositories?**

The project's goal is to demonstrate pattern understanding, not infrastructure skill. In-memory repositories eliminate database noise and keep the focus on design decisions.

**Why a hand-rolled event bus?**

Using MediatR would delegate the Observer pattern to a library, hiding the mechanism. Writing `InProcessEventBus` directly keeps the pattern visible and makes the publish/subscribe relationship explicit in the code.

**Simple Factory, not Factory Method**

The factory implementation is a Simple Factory — a class with a method that resolves the correct strategy by assignment type. This is not the GoF Factory Method pattern, which uses inheritance and subclass responsibility. The Simple Factory is the right fit here; Factory Method would require subclassing `GradingService` without solving any real problem in this domain. The distinction is documented in `docs/notes.md`.

---

## Repository Structure

```text
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

## How to Run

```bash
cd src/LMS.DesignPatterns/API
dotnet run
```

Open `https://localhost:{port}/swagger` to access the Swagger UI.

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Confirm the application is running |
| POST | `/assignments` | Create an assignment with a type and answer key |
| GET | `/assignments/{id}` | Retrieve an assignment |
| POST | `/submissions` | Submit an answer for an assignment |
| GET | `/submissions/{id}` | Retrieve a submission with its grade |
| POST | `/submissions/{id}/grade` | Grade a submission — triggers the full chain |

---

## Workflow Walkthrough

The following sequence exercises all three patterns end-to-end. Run it in Swagger UI.

**Step 1 — Create an assignment**

```http
POST /assignments
{
  "title": "Introduction to Algorithms — Quiz 1",
  "assignmentType": "Quiz",
  "maxScore": 100,
  "answerKey": "B"
}
```

**Step 2 — Submit an answer**

```http
POST /submissions
{
  "studentId": "<student-id>",
  "assignmentId": "<assignment-id>",
  "answer": "B"
}
```

**Step 3 — Grade the submission**

```http
POST /submissions/{submissionId}/grade
```

This single call exercises all three patterns:

- The **Factory** resolves `QuizGradingStrategy` from the assignment type.
- The **Strategy** executes the quiz-specific grading algorithm.
- The **Observer** publishes `AssignmentGradedEvent`; three subscribers react independently — a notification is logged, the student's progress is updated, and an audit entry is written.

**Step 4 — Verify the result**

```http
GET /submissions/{submissionId}
```

The response includes the grade, score, and feedback written by the strategy.

---

## Documentation

`docs/notes.md` contains personal study notes for all six chapters of *Head First Design Patterns*, in chapter order. Each section covers: pattern overview, the problem it solves, key concepts, trade-offs, and personal notes. Sections for implemented patterns include a "How it appears in this project" callout pointing to the relevant files.

| Chapter | Pattern | Status |
|---|---|---|
| 1 | Strategy | Implemented — `Application/Grading/` |
| 2 | Observer | Implemented — `Application/Events/` and `Subscribers/` |
| 3 | Decorator | Documented only |
| 4 | Factory | Implemented — `Application/Factory/` |
| 5 | Singleton | Documented only |
| 6 | Command | Documented only |
