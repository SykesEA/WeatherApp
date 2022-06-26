using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using static WeatherApplicationProject.WeatherEntity;

namespace WeatherApplicationProject
{
    public class Weather
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Weather> _logger;
        WeatherInfo _weatherInfo = new WeatherInfo();
        List<WeatherInfo> _weatherInfos = new List<WeatherInfo>();
        public async void Run()
        {
            
            _logger.LogInformation("This App was built to deliver accurate weather data");
            Console.WriteLine("Enter your zipcode? ");
            string? zipCode = Console.ReadLine();
            bool isSuccess = await PrintValidate(zipCode);
            if (isSuccess)
                _logger.LogInformation("Success");
        }
        public Weather(IConfiguration configuration, ILogger<Weather> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        private async Task<bool> PrintValidate(string? zipCode)
        {
            if(zipCode == null || zipCode == "")
            {
                _logger.LogWarning("Please input zipcode");
                Environment.Exit(1);
            }
            try
            {
                var weatherData = await getWeatherData(zipCode);
                _logger.LogInformation("Weather Data {@weatherData}" , weatherData.current);
                bool ifRaining = false;
                Console.WriteLine("Should I go outside?");
                if (weatherData.current.weather_descriptions != null)
                {
                    if (weatherData.current.weather_descriptions.Contains("Raining"))
                    {
                        ifRaining = true;
                        _logger.LogInformation("Yes");

                    }
                    else
                    {
                        ifRaining = false;
                        _logger.LogInformation("No");
                    }
                }
                Console.WriteLine("Should I wear sunscreen?");
                if (weatherData.current.uv_index > 3)
                {
                    _logger.LogInformation("Yes");
                }
                else
                {
                    _logger.LogInformation("No");
                }
                Console.WriteLine("Can I fly my kite?");
                if (!ifRaining && weatherData.current.wind_speed > 15)
                {
                    _logger.LogInformation("Yes");
                }
                else
                {
                    _logger.LogInformation("No");
                }
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogWarning("Error in Validation ", ex);
                return false;
            }
            
        }
        private async Task<WeatherInfo> getWeatherData(string? zipCode)
        {
            _weatherInfo = new WeatherInfo();
            try
            {
                Log.Information("Connecting to API");
                using (var client = new HttpClient())
                {
                    string weatherUrl = _configuration.GetSection("AppSettings").GetSection("WeatherAPI").Value;
                    string apiKey = _configuration.GetSection("AppSettings").GetSection("WeatherApiKey").Value;

                    HttpRequestMessage request = new HttpRequestMessage();
                    request.RequestUri = new Uri(weatherUrl + $"current?access_key={apiKey}&query={zipCode}");
                    request.Method = HttpMethod.Get;

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = client.Send(request);
                    if (response.IsSuccessStatusCode)
                    {
                        dynamic result = await response.Content.ReadAsStringAsync();
                        _weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result);

                    }
                    return _weatherInfo;
                }
            }
            catch(Exception ex)
            {
                Log.Fatal("API error ", ex.Message.ToString());
                throw;
               
            }
          

            
        }

        

    }
}
