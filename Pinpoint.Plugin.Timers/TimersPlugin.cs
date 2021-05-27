using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Timers.Results;

namespace Pinpoint.Plugin.Timers
{
    public class TimersPlugin: IPlugin
    {
        private readonly StopwatchService _stopWatchService;
        public PluginMeta Meta { get; set; } = new ("Timers plugín", PluginPriority.Highest);
        public PluginSettings UserSettings { get; set; } = new();

        public TimersPlugin()
        {
            _stopWatchService = StopwatchService.GetInstance();
        }
        public Task<bool> Activate(Query query)
        {
            return Task.FromResult(query.Parts.Length > 0 && query.Parts[0] == "timer" ||
                                   query.Parts[0] == "stopwatch");
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            if (query.Parts[0] == "stopwatch")
            {
                yield return new AddStopwatchResult();

                foreach (var stopwatchModel in _stopWatchService.GetStopwatches())
                {
                    yield return new StopwatchResult(stopwatchModel);
                }
            }
        }
    }

    public class StopwatchModel
    {
        public Stopwatch Stopwatch { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
