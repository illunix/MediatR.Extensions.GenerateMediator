namespace MediatR.Extensions.GenerateMediator.Example;

[GenerateMediator]
public partial class GetWeatherForecast
{
    public sealed partial record Query;

    public record WeatherForecast(DateTime Date, int TemperatureC, string Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public static async Task<IReadOnlyList<WeatherForecast>> Handler(Query request)
        => Enumerable.Range(1, 5).Select(index => new WeatherForecast(
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            Summaries[Random.Shared.Next(Summaries.Length)]
        ))
        .ToArray();
}
