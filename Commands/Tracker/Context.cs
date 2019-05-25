using System.Collections.Generic;
using System;
using RK800.Save;
using System.Timers;

namespace RK800.Commands.Tracker
{
    public class Context
    {
        private static Timer Timer = new Timer(60000)
        {
            AutoReset = true,
            Enabled = true,
        };

        static Context()
        {
            Timer.Elapsed += Tracker.TimeTracker.CheckTime;
        }
    }
}
