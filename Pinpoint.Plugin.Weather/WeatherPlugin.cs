﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Weather.Models;

namespace Pinpoint.Plugin.Weather
{
    public class WeatherPlugin : IPlugin
    {
        private const string DefaultCityKey = "Default city";
        private const string Description = "Look up weather forecasts.\n\nExamples: \"weather <location>\" or \"weather\" if default location is set";
        private readonly Dictionary<string, List<WeatherDayModel>> _weatherCache = new Dictionary<string, List<WeatherDayModel>>();

        public PluginMeta Meta { get; set; } = new PluginMeta("Weather", Description, PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();
        
        public async Task<bool> TryLoad()
        {
            UserSettings.Put(DefaultCityKey, string.Empty);
            return true;
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            var hasWeatherPrefix = query.Parts[0].ToLower().Equals("weather");
            if (!hasWeatherPrefix)
            {
                return false;
            }

            var defaultCity = UserSettings.Str(DefaultCityKey);
            if (!string.IsNullOrEmpty(defaultCity))
            {
                // "weather"
                return query.Parts.Length == 1;
            }

            // "weather <location>"
            return query.Parts.Length >= 2;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            string location;
            if (query.Parts.Length == 1)
            {
                location = UserSettings.Str(DefaultCityKey);
            }
            else
            {
                location = string.Join(" ", query.Parts[1..]).Trim();
            }

            if (_weatherCache.ContainsKey(location))
            {
                foreach (var weatherDayModel in _weatherCache[location])
                {
                    yield return new WeatherResult(weatherDayModel);
                }

                yield break;
            }
            
            var weather = await LookupWeather(location);
            if (weather == null)
            {
                yield break;
            }

            _weatherCache[location] = weather;

            foreach(var weatherDayModel in weather)
            {
                yield return new WeatherResult(weatherDayModel);
            }
        }

        private async Task<List<WeatherDayModel>> LookupWeather(string location)
        {
            var url = $"https://usepinpoint.com/api/weather/{location}";
            var httpResponse = await SendGet(url);
            if (string.IsNullOrEmpty(httpResponse))
            {
                return null;
            }

            if (httpResponse.Contains("error"))
            {
                return null;
            }

            return JObject.Parse(httpResponse)["forecast"]["forecastday"].Select(token =>
            {
                var weatherDayModel = JsonConvert.DeserializeObject<WeatherDayModel>(token["day"].ToString());
                weatherDayModel.DayOfWeek = DateTime.Parse(token["date"].ToString()).ToString("ddd").Substring(0, 2);
                weatherDayModel.Hours = token["hour"]
                    .Select(t => JsonConvert.DeserializeObject<WeatherHourModel>(t.ToString()))
                    .ToArray();
                return weatherDayModel;
            }).ToList();
        }


        private async Task<string> SendGet(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
