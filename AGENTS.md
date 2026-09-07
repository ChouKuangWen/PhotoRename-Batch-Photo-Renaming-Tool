# AGENTS.md

## 1. Project Context

This repository contains a C# / .NET implementation of a photo renaming tool.

The original implementation was written in Python and is located at:

`legacy/photo_rename.py`

The functional requirements are defined in:

`docs/Requirements.md`

---

## 2. Source of Truth

When implementing features, follow this priority:

1. Explicit user requirements.
2. `docs/Requirements.md`.
3. Existing C# implementation.
4. `legacy/photo_rename.py` as a behavioral reference.

The Python implementation is a reference implementation, not a required architecture.

Do not blindly copy Python code structure into C#.

---

## 3. Development Workflow

Before implementing a new feature:

1. Read the relevant requirements.
2. Inspect the existing project structure.
3. Inspect related existing code.
4. Identify dependencies and affected components.
5. Propose a small implementation plan.
6. Wait for human approval.
7. Implement only the approved task.
8. Run the relevant tests.
9. Report the changes and test results.

Do not begin a large implementation without first presenting a plan.

---

## 4. Scope Control

Keep each task small and focused.

Do not:

* Implement unrelated features.
* Add features that are not in the requirements.
* Introduce unnecessary frameworks.
* Introduce unnecessary NuGet packages.
* Refactor unrelated code.
* Change existing behavior without approval.

If a requirement is ambiguous, ask for clarification instead of making a major assumption.

---

## 5. Architecture Principles

Use a simple and maintainable architecture.

Business logic should not be tightly coupled to:

* UI code.
* Console input/output.
* File dialogs.
* Framework-specific presentation code.

Prefer separating:

* Domain models.
* Business logic.
* File system operations.
* Application/UI concerns.

Do not introduce complex architecture unless the project requirements justify it.

---

## 6. C# Development Rules

Follow standard modern C# conventions.

Prefer:

* Clear class and method names.
* Small focused classes.
* Dependency injection when it provides real value.
* `async/await` when operations are genuinely asynchronous.
* Strongly typed models.
* Explicit error handling.
* Nullable reference types where appropriate.

Avoid:

* Excessive abstraction.
* God classes.
* Static global state.
* Magic numbers.
* Duplicated business logic.
* Unnecessary design patterns.

The code should remain understandable to a developer who is learning C#/.NET.

---

## 7. File Safety

This project operates on real user files.

File operations must be treated as potentially destructive.

Before implementing file operations, consider:

* Existing destination files.
* File access permissions.
* Missing files.
* Missing JSON files.
* File name conflicts.
* Partial failures.
* Exception handling.

Do not silently delete or overwrite user files unless the requirement explicitly allows it.

Prefer safe failure over data loss.

---

## 8. Testing

Business logic should be testable independently from the UI and file system where practical.

New business logic should include appropriate Unit Tests.

At minimum, test:

* Filename generation.
* Number formatting.
* Increment modes.
* Image extension validation.
* JSON pairing.
* Collision handling.
* Invalid input.

Tests should be deterministic and should not depend on a developer's personal file system.

---

## 9. Legacy Python Code

`legacy/photo_rename.py` is reference material.

Do not modify it unless explicitly requested.

When behavior differs between the Python implementation and the written requirements:

1. Report the difference.
2. Do not silently choose one behavior.
3. Ask for clarification if the difference materially affects implementation.

---

## 10. Git Workflow

The repository uses feature branches.

Do not make development changes directly on `main`.

Feature branches should use the following naming convention:

`feature/<short-description>`

Examples:

* `feature/agent-setup`
* `feature/csharp-project`
* `feature/photo-scanner`
* `feature/rename-service`
* `feature/unit-tests`

Keep commits small and focused.

Do not create Git commits, push branches, merge branches, or create Pull Requests unless explicitly requested by the human developer.

---

## 11. Human Approval

The human developer is responsible for:

* Final requirements.
* Architecture decisions.
* Security decisions.
* Approving implementation plans.
* Reviewing AI-generated changes.
* Approving Git commits and merges.

The AI Agent assists with:

* Code analysis.
* Planning.
* Implementation.
* Testing.
* Refactoring proposals.
* Documentation.

The AI Agent must not assume approval for major architectural or behavioral changes.

---

## 12. Communication

When completing a task, report:

1. What was changed.
2. Which files were changed.
3. What tests were executed.
4. Test results.
5. Any remaining risks or issues.

If a requested change conflicts with the requirements, explain the conflict before implementing it.
