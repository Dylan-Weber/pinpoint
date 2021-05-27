using System;
using System.Diagnostics;

namespace Pinpoint.Plugin.Timers.Results
{
    public sealed class StopwatchResult: AbstractTimerResult
    {
        private readonly Stopwatch _stopwatch;
        public override TimeSpan ElapsedTime => TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds);

        public StopwatchResult(StopwatchModel stopwatchModel)
        {
            _stopwatch = stopwatchModel.Stopwatch;
            CreatedAt = stopwatchModel.CreatedAt;

            Title = ElapsedTime.ToString("g");
            Subtitle = $"Added at {CreatedAt}";
            //TODO: Add delete option
        }
        public override void OnSelect()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
            }
            else
            {
                _stopwatch.Start();
            }
        }
    }
}