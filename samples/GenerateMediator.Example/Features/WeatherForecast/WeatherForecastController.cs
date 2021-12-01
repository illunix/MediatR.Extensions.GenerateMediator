using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenerateMediator.Example.Features.WeatherForecast
{
    [Route("[controller]")]
    public class WeatherForecastController : Controller
    {
        private readonly IMediator _mediator;

        public WeatherForecastController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Get.Query query)
            => Ok(await _mediator.Send(query));
    }
}
