# Learning-Management-System
# Project Overview

**Learning Management System (LMS) Design Patterns Showcase**

This repository is a learning-focused project built to demonstrate practical understanding of the first six chapters of *Head First Design Patterns*. Instead of forcing every pattern into a single application, the project applies only the patterns that naturally solve real business problems within a simplified Learning Management System domain.

The system allows students to submit assignments, process submissions, calculate grades, and trigger post-grading activities such as notifications and progress tracking.

The project intentionally focuses on a small, realistic domain rather than a feature-complete LMS. The emphasis is on understanding the design problems, identifying code smells, and applying appropriate design patterns to improve maintainability, extensibility, and separation of concerns.

---

# Repository Goal

The primary goal of this repository is to demonstrate:

1. Understanding of the design problems that motivated the creation of design patterns.
2. Ability to recognize when a pattern is appropriate and when it is not.
3. Practical application of design patterns in a realistic business domain.
4. Evolution of code from a naïve implementation to a more flexible and maintainable design.

This repository is **not** intended to be a catalog of design patterns or a showcase where every pattern is artificially inserted into the application.

Instead, it follows a problem-driven approach:

> Use a design pattern only when it provides a meaningful solution to a real design problem.

---

# Applied Patterns

The application focuses on three patterns that naturally fit the domain.

## Strategy Pattern

Different assignment types require different grading algorithms.

Examples:

* Quiz Assignments
* Programming Assignments
* Essay Assignments

Each assignment type uses its own grading strategy without modifying existing grading logic.

**Problem solved:**
Avoid large conditional statements and make grading behavior extensible.

---

## Factory Pattern

The system must select and create the correct grading strategy based on the assignment type.

A factory centralizes the creation logic and prevents the application from directly instantiating concrete strategy implementations throughout the codebase.

**Problem solved:**
Encapsulate object creation and reduce coupling between consumers and implementations.

---

## Observer Pattern

After an assignment is graded, multiple business processes need to react:

* Send a notification to the student
* Update learning progress
* Record audit logs
* Update analytics

Instead of tightly coupling these operations to the grading workflow, the system publishes a domain event and interested components subscribe to it.

**Problem solved:**
Reduce coupling between the grading process and downstream business actions.

---

# Repository Structure

```text
LearningManagementSystem-DesignPatterns

README.md

docs/
├── 01-Strategy.md
├── 02-Observer.md
├── 03-Decorator.md
├── 04-Factory.md
├── 05-Singleton.md
└── 06-Command.md

src/
├── LMS.BeforePatterns/
└── LMS.AfterPatterns/
```

---

# Documentation

The `docs` folder contains personal study notes and summaries for the first six chapters of *Head First Design Patterns*.

These documents include:

* Key concepts
* Problems addressed by each pattern
* Trade-offs
* Lessons learned
* Small examples and observations

Not every documented pattern is implemented in the application. This is intentional and reflects a real-world engineering mindset where patterns are selected based on business needs rather than applied for demonstration purposes.

---

# What This Project Demonstrates

By reviewing this repository, readers should be able to see:

* The original design challenges.
* The limitations of a naïve implementation.
* How design patterns improve the architecture.
* Why certain patterns were selected while others were not.
* A practical example of applying object-oriented design principles in a real business domain.

The ultimate objective is to demonstrate **understanding, decision-making, and pattern selection**, rather than simply showing knowledge of pattern definitions.
