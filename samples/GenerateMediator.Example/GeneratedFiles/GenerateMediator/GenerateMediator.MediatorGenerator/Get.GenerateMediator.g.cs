
using MediatR;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace GenerateMediator.Example.Features.WeatherForecasts
{ 
    public static partial class Get 
    {       
        
public  partial record Query : IRequest<GenerateMediator.Example.Features.WeatherForecasts.Get.Model> { } 



public class _QueryHandler : IRequestHandler<Query, GenerateMediator.Example.Features.WeatherForecasts.Get.Model>
{
    

    public _QueryHandler()
    {
        
    }

    public async Task<GenerateMediator.Example.Features.WeatherForecasts.Get.Model> Handle(Query request, CancellationToken cancellationToken)  
        => await QueryHandler(request);
}
        
    } 
}