using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Timers
{
    public class TimersPlugin: IPlugin
    {
        private readonly TimerService _timerService;
        private readonly StopwatchService _stopWatchService;
        public PluginMeta Meta { get; set; } = new PluginMeta("Timers plugín", PluginPriority.Highest);
        public PluginSettings UserSettings { get; set; }

        public TimersPlugin()
        {
            _stopWatchService = new StopwatchService();
            _timerService = new TimerService();
        }
        public Task<bool> Activate(Query query)
        {
            return Task.FromResult(query.Parts.Length > 0 && query.Parts[0] == "timer");
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            foreach (var stopwatch in _stopWatchService.GetStopwatches())
            {
            }
        }
    }

    public abstract class AbstractTimerResult : AbstractFontAwesomeQueryResult
    {
        public double ElapsedTime { get; set; }
        public DateTime CreatedAt { get; set; }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_Clock;
    }

    public class StopwatchResult: AbstractTimerResult
    {
        
        public override void OnSelect()
        {
            
        }
    }

    public class StopwatchService
    {
        private readonly List<StopwatchModel> _stopwatches;

        public StopwatchService()
        {
            _stopwatches = new List<StopwatchModel>();
        }

        public IEnumerable<StopwatchModel> GetStopwatches() => _stopwatches;

        public void AddStopwatch()
        {
            var stopwatch = new Stopwatch();
            var stopwatchModel = new StopwatchModel {CreatedAt = DateTime.Now, Stopwatch = stopwatch};

            stopwatch.Start();

            _stopwatches.Add(stopwatchModel);
        }
    }

    public class StopwatchModel
    {
        public Stopwatch Stopwatch { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TimerService
    {
        private readonly Dictionary<string, Timer> _timers;

        public TimerService()
        {
            _timers = new Dictionary<string, Timer>();
        }

        public IEnumerable<TimerWithId> GetTimers()
        {
            return _timers.Select((keyValuePair) => new TimerWithId(keyValuePair.Key, keyValuePair.Value));
        }

        public bool TryGetTimer(string timerId, out Timer timer)
        {
            if (_timers.ContainsKey(timerId))
            {
                timer = _timers[timerId];
                return true;
            }

            timer = null;
            return false;
        }

        public string AddTimer(int seconds, Action<object, ElapsedEventArgs> onFinished)
        {
            var timer = new Timer(seconds);
            var timerId = DateTime.Now.ToString("G");

            timer.Elapsed += (sender, e) =>
            {
                onFinished(sender, e);
                _timers.Remove(timerId);
            };

            _timers.Add(timerId, timer);

            return timerId;
        }
    }

    public class TimerWithId
    {
        public string Id { get; set; }
        public Timer Timer { get; set; }

        public TimerWithId(string id, Timer timer)
        {
            Id = id;
            Timer = timer;
        }
    }
}
