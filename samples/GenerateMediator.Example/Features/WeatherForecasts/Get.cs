using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GenerateMediator.Example.Features.WeatherForecasts
{
    [GenerateMediator]
    public static partial class Get
    {
        public partial record Query() { }

        public record Model(IList<Model.WeatherForecast> WeatherForecasts)
        {
            public record WeatherForecast(DateTime Date, double TemperatureC);
        }

        public static async Task<Model> QueryHandler(Query query)
        {
            var weatherForecasts = new List<Model.WeatherForecast>
            {
                new Model.WeatherForecast(DateTime.Now, 18),
                new Model.WeatherForecast(DateTime.Now, 2),
                new Model.WeatherForecast(DateTime.Now, 0),
                new Model.WeatherForecast(DateTime.Now, -5)
            };

            return await Task.FromResult(new Model(weatherForecasts));
        }
    }
}
