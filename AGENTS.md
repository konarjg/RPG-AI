# C# Coding Conventions

This project follows specific coding conventions that all agents must adhere to.

## Code Style

### Indentation
- Use **2 spaces** for indentation.
- Do not use tabs.

### Brackets
- Use **K&R style** (One True Brace Style).
- Opening braces `{` should be on the same line as the statement declaration.
- Closing braces `}` should be on their own line.

Example:
```csharp
if (character is null) {
  return NotFound();
}
```

### Namespaces
- Use **file-scoped namespaces**.
- Place `using` directives *after* the namespace declaration (if present) or at the top of the file.

Example:
```csharp
namespace RpgAI.Controllers;

using Application.Providers;
```

## Language Features

### Constructors
- Use **primary constructors** for classes where appropriate, especially for dependency injection in controllers.

Example:
```csharp
public class CharactersController(ICharacterProvider characterProvider) : ControllerBase {
```

### Object Initialization
- Use **target-typed `new`** when the type is inferred.

Example:
```csharp
User test = new() {
  Id = id
};
```

### Typing
- **Explicit Typing**: Use explicit types for ALL variables.
- **NEVER use `var`**. Explicit types improve readability and prevent ambiguity in this project.

Example:
```csharp
User user = userProvider.GetUser(id); // Correct
var user = userProvider.GetUser(id); // Incorrect
```
Product product = entityProvider.Get<Product>(id);
```

### Properties
- Use the `required` modifier for properties that must be initialized.

Example:
```csharp
public required Guid Id { get; set; }
```

## General
- Remove unused `using` directives.
- Ensure all new files follow these conventions.
