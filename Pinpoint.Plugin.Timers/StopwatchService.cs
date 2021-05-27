using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pinpoint.Plugin.Timers
{
    public class StopwatchService
    {
        private static StopwatchService _instance;
        private readonly List<StopwatchModel> _stopwatches;
        private StopwatchService()
        {
            _stopwatches = new List<StopwatchModel>();
        }

        public static StopwatchService GetInstance() => _instance ??= new StopwatchService();

        public IEnumerable<StopwatchModel> GetStopwatches() => _stopwatches;

        public void AddStopwatch()
        {
            var stopwatch = new Stopwatch();
            var stopwatchModel = new StopwatchModel {CreatedAt = DateTime.Now, Stopwatch = stopwatch};

            stopwatch.Start();

            _stopwatches.Add(stopwatchModel);
        }
    }
}