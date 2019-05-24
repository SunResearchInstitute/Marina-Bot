using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace RK800.Commands
{
    public class TrackerContext
    {
        public static Dictionary<ulong, DateTime> Trackers = new Dictionary<ulong, DateTime>();
    }
}
