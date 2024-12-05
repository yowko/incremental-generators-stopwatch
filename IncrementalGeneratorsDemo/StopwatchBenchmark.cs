using System.Diagnostics;
using IncrementalGenerators_net9;

namespace IncrementalGeneratorsDemo;

public partial class StopwatchBenchmark
{
    [Timed]
    public TimeSpan Old_StartNew()
    {
        Stopwatch sw = Stopwatch.StartNew();
        return sw.Elapsed;
    }
    [Timed]
    public TimeSpan New_GetTimestamp()
    {
        long timestamp = Stopwatch.GetTimestamp();
        return Stopwatch.GetElapsedTime(timestamp);
    }
}