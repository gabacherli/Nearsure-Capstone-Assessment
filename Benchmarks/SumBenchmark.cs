using System;
using System.Diagnostics;
using System.Linq;

namespace DevSample.Benchmarks
{
    /// <summary>
    /// Benchmarks different approaches to summing sample values.
    /// Compares: foreach loop, LINQ Sum(), and manual for loop.
    /// </summary>
    class SumBenchmark
    {
        private readonly DateTime _sampleStartDate;
        private readonly TimeSpan _sampleIncrement;
        private readonly int _samplesToLoad;

        public SumBenchmark(DateTime sampleStartDate, TimeSpan sampleIncrement, int samplesToLoad)
        {
            _sampleStartDate = sampleStartDate;
            _sampleIncrement = sampleIncrement;
            _samplesToLoad = samplesToLoad;
        }

        /// <summary>
        /// Runs the benchmark comparing different sum methods.
        /// </summary>
        public void Run()
        {
            Console.WriteLine("\n========== SUM METHOD BENCHMARK ==========\n");

            // Create a sample generator with test data
            SampleGenerator testGenerator = new SampleGenerator(_sampleStartDate, _sampleIncrement);
            testGenerator.LoadSamples(_samplesToLoad);

            const int iterations = 2000;
            Stopwatch sw = new Stopwatch();

            // Benchmark 1: foreach loop (current implementation)
            sw.Start();
            for (int iter = 0; iter < iterations; iter++)
            {
                decimal sum = 0;
                foreach (Sample s in testGenerator.Samples)
                {
                    sum += s.Value;
                }
            }
            sw.Stop();
            double foreachTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"foreach loop:        {foreachTime:F2} ms ({iterations} iterations)");

            // Benchmark 2: LINQ Sum()
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                decimal sum = testGenerator.Samples.Sum(s => (decimal)s.Value);
            }
            sw.Stop();
            double linqTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"LINQ Sum():          {linqTime:F2} ms ({iterations} iterations)");

            // Benchmark 3: Manual for loop with Count
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                decimal sum = 0;
                for (int i = 0; i < testGenerator.Samples.Count; i++)
                {
                    sum += testGenerator.Samples[i].Value;
                }
            }
            sw.Stop();
            double forLoopTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"for loop (indexed):  {forLoopTime:F2} ms ({iterations} iterations)");

            // Calculate and display results
            Console.WriteLine($"\n--- Results ---");
            double[] times = { foreachTime, linqTime, forLoopTime };
            double fastest = times.Min();
            double slowest = times.Max();

            string[] methods = { "foreach loop", "LINQ Sum()", "for loop (indexed)", "LINQ Sum() (decimal)" };
            int fastestIdx = Array.IndexOf(times, fastest);
            int slowestIdx = Array.IndexOf(times, slowest);

            Console.WriteLine($"Fastest:  {methods[fastestIdx]} ({fastest:F2} ms)");
            Console.WriteLine($"Slowest:  {methods[slowestIdx]} ({slowest:F2} ms)");
            Console.WriteLine($"\nRelative Performance (vs foreach):");
            Console.WriteLine($"  foreach loop:        1.00x (baseline)");
            Console.WriteLine($"  LINQ Sum():          {linqTime / foreachTime:F2}x");
            Console.WriteLine($"  for loop (indexed):  {forLoopTime / foreachTime:F2}x");
            Console.WriteLine($"\n=========================================\n");
        }
    }
}

