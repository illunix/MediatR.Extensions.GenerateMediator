# MediatR.Extensions.GenerateMediator
[![NuGet](https://img.shields.io/nuget/dt/MediatR.Extensions.GenerateMediator.svg)](https://www.nuget.org/packages/MediatR.Extensions.GenerateMediator) 
[![NuGet](https://img.shields.io/nuget/vpre/MediatR.Extensions.GenerateMediator.svg)](https://www.nuget.org/packages/MediatR.Extensions.GenerateMediator)

Generate commands, queries and handlers  for MediatR

## Installation

```
PM> Install-Package MediatR.Extensions.GenerateMediator
```

# Usage
## Command
```csharp
[GenerateMediator]
public partial class AddEmployee
{
    public sealed partial record Command;

    public static async Task Handler(Command command)
    {
        // logic
    }
}
```

## Query
```csharp
[GenerateMediator]
public partial class GetEmployees
{
    public sealed partial record Query;
    
    public sealed record Employee(string FirstName, string LastName);

    public static async Task<IEnumerable<Employee>> Handler()
    {
        // logic
    }
}
```

## Request Validator
If you want add validation to your request first thing you need to do is install fluent validation package
```
PM> Install-Package FluentValidation.AspNetCore
```
To use valdation on your request just use ```AddValidation``` method in your request, it should look something like this
```csharp
public sealed partial record Query(string Id)
{
    public static void AddValidation(AbstractValidator<Query> v)
        => v.RuleFor(x => x.Id).NotEmpty();
}
```
