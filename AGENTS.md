# Agent Instructions

This document provides instructions for agents working on this repository.

## Coding Style

- **Indentation**: Use 2 spaces for indentation.
- **Brackets**: Use Java-style brackets, where the opening brace is on the same line as the statement.
- **Constructors**: Use primary constructors for dependency injection in .NET 9.
- **Variable Declaration**: Do not use the `var` keyword. Specify the type explicitly on the left-hand side of the assignment.

## Testing

- **Framework**: Use xUnit for unit tests.
- **Mocking**: Use NSubstitute for creating test doubles.
- **Assertions**: Use FluentAssertions for writing assertions.
