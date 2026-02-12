using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Optimization
{
    /// <summary>
    /// Custom performance profiler for identifying VFX bottlenecks.
    /// Tracks timing, call counts, and provides detailed statistics.
    /// </summary>
    public class PerformanceProfiler : ModSystem
    {
        private class ProfileSample
        {
            public string Name;
            public Stopwatch Timer;
            public List<double> Times;
            public int CallCount;
            public double TotalTime;
            public double MinTime;
            public double MaxTime;

            public ProfileSample(string name)
            {
                Name = name;
                Timer = new Stopwatch();
                Times = new List<double>();
                Reset();
            }

            public void Reset()
            {
                CallCount = 0;
                TotalTime = 0;
                MinTime = double.MaxValue;
                MaxTime = 0;
            }

            public double AverageTime => CallCount > 0 ? TotalTime / CallCount : 0;
        }

        private Dictionary<string, ProfileSample> samples;
        private Stack<ProfileSample> activeStack;
        private StringBuilder stringBuilder;
        private int framesSinceLastPrint;
        private const int PrintIntervalFrames = 60; // Print every second

        private static PerformanceProfiler _instance;
        public static PerformanceProfiler Instance => _instance;

        public bool Enabled { get; set; } = false;
        public bool AutoPrint { get; set; } = false;

        public override void Load()
        {
            _instance = this;
            samples = new Dictionary<string, ProfileSample>();
            activeStack = new Stack<ProfileSample>();
            stringBuilder = new StringBuilder(2048);
            framesSinceLastPrint = 0;
        }

        public override void Unload()
        {
            _instance = null;
            samples?.Clear();
            samples = null;
            activeStack?.Clear();
            activeStack = null;
            stringBuilder = null;
        }

        public override void PostUpdateEverything()
        {
            if (!Enabled || !AutoPrint)
                return;

            framesSinceLastPrint++;
            if (framesSinceLastPrint >= PrintIntervalFrames)
            {
                PrintResults();
                ResetFrame();
                framesSinceLastPrint = 0;
            }
        }

        /// <summary>
        /// Begin profiling a named section.
        /// </summary>
        public void Begin(string name)
        {
            if (!Enabled)
                return;

            if (!samples.ContainsKey(name))
            {
                samples[name] = new ProfileSample(name);
            }

            var sample = samples[name];
            sample.Timer.Restart();
            activeStack.Push(sample);
        }

        /// <summary>
        /// End profiling a named section.
        /// </summary>
        public void End(string name)
        {
            if (!Enabled)
                return;

            if (activeStack.Count == 0)
            {
                Main.NewText($"[Profiler Error] End called without Begin for {name}", Microsoft.Xna.Framework.Color.Red);
                return;
            }

            var sample = activeStack.Pop();

            if (sample.Name != name)
            {
                Main.NewText($"[Profiler Error] Expected {sample.Name}, got {name}", Microsoft.Xna.Framework.Color.Red);
                return;
            }

            sample.Timer.Stop();
            double elapsed = sample.Timer.Elapsed.TotalMilliseconds;

            sample.CallCount++;
            sample.TotalTime += elapsed;
            sample.MinTime = Math.Min(sample.MinTime, elapsed);
            sample.MaxTime = Math.Max(sample.MaxTime, elapsed);
            sample.Times.Add(elapsed);

            // Keep history limited
            if (sample.Times.Count > 1000)
                sample.Times.RemoveAt(0);
        }

        /// <summary>
        /// Create a profiling scope that automatically ends when disposed.
        /// Usage: using (Profiler.Scope("name")) { ... }
        /// </summary>
        public ProfileScope Scope(string name)
        {
            return new ProfileScope(this, name);
        }

        /// <summary>
        /// Get results for a specific sample.
        /// </summary>
        public (double avg, double min, double max, int calls) GetResults(string name)
        {
            if (!samples.ContainsKey(name))
                return (0, 0, 0, 0);

            var sample = samples[name];
            return (sample.AverageTime, sample.MinTime, sample.MaxTime, sample.CallCount);
        }

        /// <summary>
        /// Get all results sorted by total time.
        /// </summary>
        public List<(string name, double avgMs, double totalMs, int calls)> GetAllResults()
        {
            return samples.Values
                .OrderByDescending(s => s.TotalTime)
                .Select(s => (s.Name, s.AverageTime, s.TotalTime, s.CallCount))
                .ToList();
        }

        /// <summary>
        /// Print results to the game chat/console.
        /// </summary>
        public void PrintResults()
        {
            if (samples.Count == 0)
                return;

            stringBuilder.Clear();
            stringBuilder.AppendLine("=== Performance Profile ===");

            foreach (var sample in samples.Values.OrderByDescending(s => s.TotalTime).Take(10))
            {
                if (sample.CallCount == 0)
                    continue;

                stringBuilder.AppendFormat("{0}: {1:F3}ms avg, {2:F2}ms total, {3} calls\n",
                    sample.Name, sample.AverageTime, sample.TotalTime, sample.CallCount);
            }

            Main.NewText(stringBuilder.ToString(), Microsoft.Xna.Framework.Color.Cyan);
        }

        /// <summary>
        /// Get formatted results as a string.
        /// </summary>
        public string GetResultsString()
        {
            stringBuilder.Clear();
            stringBuilder.AppendLine("=== Performance Profile ===");
            stringBuilder.AppendFormat("{0,-30} {1,10} {2,12} {3,8} {4,8} {5,8}\n",
                "Name", "Avg (ms)", "Total (ms)", "Calls", "Min", "Max");
            stringBuilder.AppendLine(new string('-', 85));

            foreach (var sample in samples.Values.OrderByDescending(s => s.TotalTime))
            {
                if (sample.CallCount == 0)
                    continue;

                stringBuilder.AppendFormat("{0,-30} {1,10:F3} {2,12:F2} {3,8} {4,8:F3} {5,8:F3}\n",
                    sample.Name, sample.AverageTime, sample.TotalTime,
                    sample.CallCount, sample.MinTime, sample.MaxTime);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Reset all samples for a new profiling session.
        /// </summary>
        public void ResetFrame()
        {
            foreach (var sample in samples.Values)
            {
                sample.Reset();
            }
        }

        /// <summary>
        /// Clear all profiling data.
        /// </summary>
        public void ClearAll()
        {
            samples.Clear();
            activeStack.Clear();
        }

        /// <summary>
        /// Get the number of active (nested) profile scopes.
        /// </summary>
        public int GetActiveDepth()
        {
            return activeStack.Count;
        }

        /// <summary>
        /// Disposable scope for automatic Begin/End pairing.
        /// </summary>
        public struct ProfileScope : IDisposable
        {
            private PerformanceProfiler profiler;
            private string name;

            public ProfileScope(PerformanceProfiler profiler, string name)
            {
                this.profiler = profiler;
                this.name = name;
                profiler.Begin(name);
            }

            public void Dispose()
            {
                profiler.End(name);
            }
        }
    }

    /// <summary>
    /// Memory profiler for tracking allocations and GC pressure.
    /// </summary>
    public class MemoryProfiler : ModSystem
    {
        private long lastTotalMemory;
        private long lastGCCount;
        private List<long> allocationHistory;
        private const int HistorySize = 600; // 10 seconds at 60fps

        private static MemoryProfiler _instance;
        public static MemoryProfiler Instance => _instance;

        public bool Enabled { get; set; } = false;

        public override void Load()
        {
            _instance = this;
            allocationHistory = new List<long>(HistorySize);
            lastTotalMemory = GC.GetTotalMemory(false);
            lastGCCount = GC.CollectionCount(0);
        }

        public override void Unload()
        {
            _instance = null;
            allocationHistory = null;
        }

        public override void PostUpdateEverything()
        {
            if (!Enabled)
                return;

            Sample();
        }

        /// <summary>
        /// Take a memory sample.
        /// </summary>
        public void Sample()
        {
            long currentMemory = GC.GetTotalMemory(false);
            long allocated = currentMemory - lastTotalMemory;

            if (allocated > 0)
            {
                allocationHistory.Add(allocated);

                if (allocationHistory.Count > HistorySize)
                    allocationHistory.RemoveAt(0);
            }

            lastTotalMemory = currentMemory;

            // Check for GC
            long currentGCCount = GC.CollectionCount(0);
            if (currentGCCount > lastGCCount)
            {
                Main.NewText($"[GC] Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}",
                    Microsoft.Xna.Framework.Color.Yellow);
                lastGCCount = currentGCCount;
            }
        }

        /// <summary>
        /// Print memory statistics.
        /// </summary>
        public void PrintStats()
        {
            long totalMemory = GC.GetTotalMemory(false);
            long avgAllocation = allocationHistory.Count > 0 ?
                (long)allocationHistory.Average() : 0;

            Main.NewText($"[Memory] Total: {totalMemory / 1024 / 1024:F2} MB, " +
                        $"Avg Alloc/Frame: {avgAllocation / 1024:F2} KB",
                        Microsoft.Xna.Framework.Color.Cyan);
            Main.NewText($"[GC] Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}",
                Microsoft.Xna.Framework.Color.Cyan);
        }

        /// <summary>
        /// Get current memory usage in MB.
        /// </summary>
        public float GetMemoryUsageMB()
        {
            return GC.GetTotalMemory(false) / (1024f * 1024f);
        }

        /// <summary>
        /// Get average allocation per frame in KB.
        /// </summary>
        public float GetAvgAllocationKB()
        {
            if (allocationHistory.Count == 0)
                return 0;
            return (float)allocationHistory.Average() / 1024f;
        }

        /// <summary>
        /// Force a garbage collection (use sparingly for testing only).
        /// </summary>
        public void ForceGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Main.NewText("[Memory] Forced GC complete", Microsoft.Xna.Framework.Color.Green);
        }
    }
}
