using System;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.Timers.Results
{
    public abstract class AbstractTimerResult : AbstractFontAwesomeQueryResult
    {
        public abstract TimeSpan ElapsedTime { get; }
        public DateTime CreatedAt { get; set; }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_Clock;
    }
}