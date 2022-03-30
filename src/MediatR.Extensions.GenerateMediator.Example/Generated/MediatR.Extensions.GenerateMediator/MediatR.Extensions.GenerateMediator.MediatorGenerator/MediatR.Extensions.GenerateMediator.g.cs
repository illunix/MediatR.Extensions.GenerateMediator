
namespace MediatR.Extensions.GenerateMediator.Example
{
    public partial class GetWeatherForecast : IRequestHandler<GetWeatherForecast.Query, System.Collections.Generic.IReadOnlyList<MediatR.Extensions.GenerateMediator.Example.GetWeatherForecast.WeatherForecast>>
    {
        public partial record Query : IRequest<System.Collections.Generic.IReadOnlyList<MediatR.Extensions.GenerateMediator.Example.GetWeatherForecast.WeatherForecast>> { }


        public GetWeatherForecast()
        {
            
        }
        
        public async Task<System.Collections.Generic.IReadOnlyList<MediatR.Extensions.GenerateMediator.Example.GetWeatherForecast.WeatherForecast>> Handle(Query request, CancellationToken cancellationToken) 
        {
            return await Handler(request);
        }
    }
}


namespace MediatR.Extensions.GenerateMediator.Example
{
    public partial class Test : IRequestHandler<Test.Command, MediatR.Extensions.GenerateMediator.Example.Example>
    {
        public partial record Command : IRequest<MediatR.Extensions.GenerateMediator.Example.Example> { }
        
        public Test()
        {
            
        }
        
        public async Task<MediatR.Extensions.GenerateMediator.Example.Example> Handle(Command request, CancellationToken cancellationToken) 
        {
            return await Handler(request);
        }
    }
}

