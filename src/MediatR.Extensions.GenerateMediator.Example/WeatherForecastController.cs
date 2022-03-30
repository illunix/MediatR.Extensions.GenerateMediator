using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MediatR.Extensions.GenerateMediator.Example.Controllers;

[ApiController]
[Route("api/weather-forecast")]
public class WeatherForecastController : ControllerBase
{
    private readonly IMediator _mediator;

    public WeatherForecastController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] GetWeatherForecast.Query request)
        => Ok(await _mediator.Send(request));
}