using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.CloverAPI.Weather;
using Microsoft.AspNetCore.Mvc;

namespace Doppler.CloverAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _service;

    public WeatherForecastController(IWeatherForecastService service)
    {
        _service = service;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return _service.GetForecasts();
    }
}
