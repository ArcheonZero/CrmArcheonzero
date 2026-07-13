using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CrmArcheonzero.Services
{
    public static class PerformanceMonitor
    {
        private static readonly Dictionary<string, Stopwatch> _timers = new();
        private static readonly Dictionary<string, List<long>> _metrics = new();

        public static void Start(string operation)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _timers[operation] = stopwatch;
        }

        public static void Stop(string operation)
        {
            if (_timers.TryGetValue(operation, out var stopwatch))
            {
                stopwatch.Stop();
                var elapsed = stopwatch.ElapsedMilliseconds;

                if (!_metrics.ContainsKey(operation))
                    _metrics[operation] = new List<long>();

                _metrics[operation].Add(elapsed);
                _timers.Remove(operation);

                if (elapsed > 1000)
                    Console.WriteLine($"⚠️ Медленная операция: {operation} ({elapsed}ms)");
            }
        }

        public static Dictionary<string, long> GetAverageTimes()
        {
            var result = new Dictionary<string, long>();
            foreach (var kvp in _metrics)
                result[kvp.Key] = (long)kvp.Value.Average();
            return result;
        }

        public static void ClearMetrics() => _metrics.Clear();
    }
}