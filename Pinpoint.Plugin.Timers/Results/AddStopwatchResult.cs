using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Timers.Results
{
    public class AddStopwatchResult: AbstractFontAwesomeQueryResult
    {
        public AddStopwatchResult(): base("Start stopwatch")
        {
            
        }
        public override void OnSelect()
        {
            StopwatchService.GetInstance().AddStopwatch();
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_Clock;
    }
}