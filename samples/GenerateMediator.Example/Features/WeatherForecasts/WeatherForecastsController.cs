using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenerateMediator.Example.Features.WeatherForecasts
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherForecastsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WeatherForecastsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromForm]Get.Query query)
        {
            var weatherForecasts = await _mediator.Send(query);

            return Ok(weatherForecasts);
        }
    }
}
