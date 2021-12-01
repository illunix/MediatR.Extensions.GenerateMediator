using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GenerateMediator.Example.Features.WeatherForecast
{
    [GenerateMediator]
    public static partial class Get
    {
        public partial record Query;

        public record WeatherForecast(DateTime Date, double TemperatureC);

        public static async Task<IReadOnlyList<WeatherForecast>> QueryHandler(Query query)
        {
            var weatherForecasts = new List<WeatherForecast>
            {
                new WeatherForecast(DateTime.Now, 18),
                new WeatherForecast(DateTime.Now, 2),
                new WeatherForecast(DateTime.Now, 0),
                new WeatherForecast(DateTime.Now, -5)
            };

            return await Task.FromResult(weatherForecasts);
        }
    }
}
