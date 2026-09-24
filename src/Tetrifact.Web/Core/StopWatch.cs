using System;

namespace Madscience.Time
{
    public class StopWatch
    {
        private DateTime _start;

        public StopWatch()
        {
            _start = DateTime.UtcNow;
        }

        public double ElapsedMsec()
        {
            return (DateTime.UtcNow - _start).TotalMilliseconds;
        }

        public double ElapsedSec()
        {
            return (DateTime.UtcNow - _start).TotalSeconds;
        }

        public double ElapsedMin()
        {
            return (DateTime.UtcNow - _start).TotalMinutes;
        }
    }
}