# GenerateMediator
[![NuGet](https://img.shields.io/nuget/dt/GenerateMediator.svg)](https://www.nuget.org/packages/GenerateMediator) 
[![NuGet](https://img.shields.io/nuget/vpre/GenerateMediator.svg)](https://www.nuget.org/packages/GenerateMediator)

Generate command, queries, validators and handlers for MediatR

## Prerequisites

Visual Studio version 16.8 and above is required as its first version to support source generators.

## Installation

```
PM> Install-Package GenerateMediator
```

## Usage

```csharp
[GenerateMediator]
public static partial class Get
{
    public partial record Query(int Id) 
    {
        public static void AddValidation(AbstractValidator<Query> v)
            => v.RuleFor(x => x.Id).NotEmpty();
    }

    public record Model(IList<Model.WeatherForecast> WeatherForecasts)
    {
        public record WeatherForecast(DateTime Date, double TemperatureC);
    }

    public static async Task<Model> QueryHandler(Query query)
    {
        var weatherForecasts = new List<Model.WeatherForecast>();

        return await Task.FromResult(new Model(weatherForecasts));
    }
}
```

When compile, following source will be injected.

```csharp
public static partial class Get 
{          
    public partial record Query : IRequest<Model> { } 

    private class QueryValidator : AbstractValidator<Query> 
    { 
        public QueryValidator()
        {
            Query.AddValidation(this); 
        }
    }   
    
    public class _QueryHandler : IRequestHandler<Query, Model>
    {
        public async Task<Model> Handle(Query request, CancellationToken cancellationToken)  
            => await QueryHandler(request);
    }
} 
```
