using System.Collections.Generic;

namespace Doppler.CloverAPI.Weather;

public interface IWeatherForecastService
{
    IEnumerable<WeatherForecast> GetForecasts();
}
