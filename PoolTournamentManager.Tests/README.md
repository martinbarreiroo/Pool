# PoolTournamentManager.Tests

This project contains unit and integration tests for the Pool Tournament Manager application.

## Test Structure

Tests are organized by feature and follow the structure of the main application:

```
PoolTournamentManager.Tests/
├── Features/
│   ├── Players/
│   │   ├── Services/
│   │   │   └── PlayerServiceTests.cs
│   ├── Matches/
│   │   ├── Services/
│   │   │   └── MatchServiceTests.cs
│   ├── Tournaments/
│   │   ├── Services/
│   │   │   └── TournamentServiceTests.cs
├── ExampleTests.cs      # Example tests showing xUnit patterns
├── GlobalUsings.cs      # Global using statements
├── README.md            # This file
```

## Testing Approach

We use the following testing frameworks and libraries:

- **xUnit** - Main testing framework
- **Moq** - For mocking dependencies
- **NSubstitute** - Alternative mocking framework for specific cases
- **Bogus** - For generating realistic test data
- **FluentAssertions** - For more readable assertions (optional)

## Writing Tests

### Naming Convention

Tests should follow the naming convention:

```
[MethodUnderTest]_[Scenario]_[ExpectedResult]
```

Examples:
- `CreatePlayerAsync_WithValidData_ReturnsSuccessfully`
- `GetPlayerByIdAsync_NonExistingPlayer_ReturnsNull`

### Test Structure

Each test should follow the AAA pattern:

1. **Arrange** - Set up test data and dependencies
2. **Act** - Call the method being tested
3. **Assert** - Verify the expected outcome

Example:

```csharp
[Fact]
public async Task GetPlayerByIdAsync_ExistingPlayer_ReturnsPlayerDto()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var player = new Player { Id = playerId, Name = "Test Player" };
    _mockDbContext.Setup(...).Returns(...);

    // Act
    var result = await _playerService.GetPlayerByIdAsync(playerId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(playerId, result.Id);
}
```

### Mocking Dependencies

Use Moq or NSubstitute to mock dependencies:

```csharp
// Example with Moq
var mockDbSet = new Mock<DbSet<Player>>();
_mockDbContext.Setup(c => c.Players).Returns(mockDbSet.Object);

// Example with NSubstitute
var storageService = Substitute.For<S3StorageService>();
storageService.GeneratePresignedUrlAsync(Arg.Any<Guid>(), Arg.Any<string>())
    .Returns((presignedUrl, objectUrl));
```

### Test Data Generation

Use Bogus for generating realistic test data:

```csharp
var playerFaker = new Faker<Player>()
    .RuleFor(p => p.Id, f => Guid.NewGuid())
    .RuleFor(p => p.Name, f => f.Name.FullName())
    .RuleFor(p => p.Email, f => f.Internet.Email());

var players = playerFaker.Generate(5); // Generate 5 random players
```

## Running Tests

### Command Line

```bash
cd PoolTournamentManager.Tests
dotnet test
```

### Visual Studio

1. Open the solution in Visual Studio
2. Right-click on the test project and select "Run Tests"

### VS Code

1. Install the .NET Core Test Explorer extension
2. Configure it to discover tests in your project
3. Use the Test Explorer panel to run tests

## Test Coverage

We aim for high test coverage of business logic, especially in service classes. When adding new features:

1. Write tests for the happy path
2. Add tests for edge cases and error conditions
3. Verify that validation works correctly

## Integration Tests

For database integration tests:
- Use `Microsoft.AspNetCore.Mvc.Testing` package
- Create a test database context with in-memory provider or test container
- Reset the database state between tests

## Recommendations

1. Write tests before implementing features (TDD) when possible
2. Focus on testing business logic rather than implementation details
3. Use test doubles (mocks, stubs) for external dependencies
4. Keep tests fast, independent, and repeatable