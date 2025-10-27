using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevSample.Benchmarks
{
    /// <summary>
    /// Benchmarks different approaches to loading samples.
    /// Compares: Insert(0), Add + Reverse, and LinkedList approaches.
    /// </summary>
    class SampleGeneratorBenchmark
    {
        private readonly DateTime _sampleStartDate;
        private readonly TimeSpan _sampleIncrement;
        private readonly int _samplesToLoad;

        public SampleGeneratorBenchmark(DateTime sampleStartDate, TimeSpan sampleIncrement, int samplesToLoad)
        {
            _sampleStartDate = sampleStartDate;
            _sampleIncrement = sampleIncrement;
            _samplesToLoad = samplesToLoad;
        }

        /// <summary>
        /// Runs the benchmark comparing different load methods.
        /// </summary>
        public void LoadSamples()
        {
            Console.WriteLine("\n========== LOAD SAMPLES BENCHMARK ==========\n");

            const int iterations = 10;
            Stopwatch sw = new Stopwatch();

            // Benchmark 1: Current implementation - Insert(0) - O(n²)
            sw.Start();
            for (int iter = 0; iter < iterations; iter++)
            {
                var list = new List<Sample>();
                DateTime date = _sampleStartDate;

                for (int i = 0; i < _samplesToLoad; i++)
                {
                    Sample s = new Sample(i == 0);
                    s.LoadSampleAtTime(date);
                    list.Insert(0, s);
                    date += _sampleIncrement;
                }
            }
            sw.Stop();
            double insertTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Insert(0):                {insertTime:F2} ms ({iterations} iterations)");

            // Benchmark 2: Add + Reverse - O(n)
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                var list = new List<Sample>();
                DateTime date = _sampleStartDate;

                for (int i = 0; i < _samplesToLoad; i++)
                {
                    Sample s = new Sample(i == 0);
                    s.LoadSampleAtTime(date);
                    list.Add(s);
                    date += _sampleIncrement;
                }

                list.Reverse();
            }
            sw.Stop();
            double addReverseTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Add + Reverse:            {addReverseTime:F2} ms ({iterations} iterations)");

            // Benchmark 3: LinkedList - O(n)
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                var linkedList = new LinkedList<Sample>();
                DateTime date = _sampleStartDate;

                for (int i = 0; i < _samplesToLoad; i++)
                {
                    Sample s = new Sample(i == 0);
                    s.LoadSampleAtTime(date);
                    linkedList.AddFirst(s);
                    date += _sampleIncrement;
                }

                // Convert back to List for consistency
                var list = linkedList.ToList();
            }
            sw.Stop();
            double linkedListTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"LinkedList.AddFirst:      {linkedListTime:F2} ms ({iterations} iterations)");

            // Benchmark 4: Pre-allocate with capacity + Add + Reverse - O(n)
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                var list = new List<Sample>(_samplesToLoad);
                DateTime date = _sampleStartDate;

                for (int i = 0; i < _samplesToLoad; i++)
                {
                    Sample s = new Sample(i == 0);
                    s.LoadSampleAtTime(date);
                    list.Add(s);
                    date += _sampleIncrement;
                }

                list.Reverse();
            }
            sw.Stop();
            double preallocatedTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Preallocated + Reverse:   {preallocatedTime:F2} ms ({iterations} iterations)");

            // Benchmark 5: Parallel sample creation with ConcurrentBag
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                var concurrentBag = new System.Collections.Concurrent.ConcurrentBag<(int index, Sample sample)>();

                Parallel.For(0, _samplesToLoad, i =>
                {
                    DateTime date = _sampleStartDate.AddTicks(_sampleIncrement.Ticks * i);
                    Sample s = new Sample(i == 0);
                    s.LoadSampleAtTime(date);
                    concurrentBag.Add((i, s));
                });

                // Sort by index descending to maintain time-descending order
                var list = concurrentBag.OrderByDescending(x => x.index).Select(x => x.sample).ToList();
            }
            sw.Stop();
            double parallelBagTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Parallel + ConcurrentBag: {parallelBagTime:F2} ms ({iterations} iterations)");

            // Benchmark 6: Parallel.ForEach with thread-safe list
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                var samples = new Sample[_samplesToLoad];

                Parallel.For(0, _samplesToLoad, i =>
                {
                    DateTime date = _sampleStartDate.AddTicks(_sampleIncrement.Ticks * i);
                    Sample s = new Sample(i == 0);
                    s.LoadSampleAtTime(date);
                    samples[i] = s;
                });

                // Reverse array to maintain time-descending order
                Array.Reverse(samples);
                var list = samples.ToList();
            }
            sw.Stop();
            double parallelArrayTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Parallel + Array:         {parallelArrayTime:F2} ms ({iterations} iterations)");

            // Calculate and display results
            Console.WriteLine($"\n--- Results ---");
            double[] times = { insertTime, addReverseTime, linkedListTime, preallocatedTime, parallelBagTime, parallelArrayTime };
            double fastest = times.Min();
            double slowest = times.Max();

            string[] methods = { "Insert(0)", "Add + Reverse", "LinkedList.AddFirst", "Preallocated + Reverse", "Parallel + ConcurrentBag", "Parallel + Array" };
            int fastestIdx = Array.IndexOf(times, fastest);
            int slowestIdx = Array.IndexOf(times, slowest);

            Console.WriteLine($"Fastest:  {methods[fastestIdx]} ({fastest:F2} ms)");
            Console.WriteLine($"Slowest:  {methods[slowestIdx]} ({slowest:F2} ms)");
            Console.WriteLine($"\nRelative Performance (vs Insert(0)):");
            Console.WriteLine($"  Insert(0):                1.00x (baseline)");
            Console.WriteLine($"  Add + Reverse:            {addReverseTime / insertTime:F2}x");
            Console.WriteLine($"  LinkedList.AddFirst:      {linkedListTime / insertTime:F2}x");
            Console.WriteLine($"  Preallocated + Reverse:   {preallocatedTime / insertTime:F2}x");
            Console.WriteLine($"  Parallel + ConcurrentBag: {parallelBagTime / insertTime:F2}x");
            Console.WriteLine($"  Parallel + Array:         {parallelArrayTime / insertTime:F2}x");
            Console.WriteLine($"\n==========================================\n");
        }

        /// <summary>
        /// Runs the benchmark comparing different validation methods.
        /// </summary>
        public void ValidateSamples()
        {
            Console.WriteLine("\n========== VALIDATE SAMPLES BENCHMARK ==========\n");

            // First, generate samples to validate
            var sampleGenerator = new SampleGenerator(_sampleStartDate, _sampleIncrement);
            sampleGenerator.LoadSamples(_samplesToLoad);

            const int iterations = 10;
            Stopwatch sw = new Stopwatch();

            // Benchmark 1: Sequential for loop (current implementation)
            sw.Start();
            for (int iter = 0; iter < iterations; iter++)
            {
                int samplesValidated = 0;

                for (int i = 0; i < sampleGenerator.Samples.Count; i++)
                {
                    if (sampleGenerator.Samples[i].ValidateSample(
                        i < sampleGenerator.Samples.Count - 1 ? sampleGenerator.Samples[i + 1] : null,
                        _sampleIncrement))
                    {
                        samplesValidated++;
                    }
                }
            }
            sw.Stop();
            double sequentialTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Sequential for loop: {sequentialTime:F2} ms ({iterations} iterations)");

            // Benchmark 2: Parallel.For
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                int samplesValidated = 0;
                object lockObj = new object();

                Parallel.For(0, sampleGenerator.Samples.Count, i =>
                {
                    if (sampleGenerator.Samples[i].ValidateSample(
                        i < sampleGenerator.Samples.Count - 1 ? sampleGenerator.Samples[i + 1] : null,
                        _sampleIncrement))
                    {
                        lock (lockObj)
                        {
                            samplesValidated++;
                        }
                    }
                });
            }
            sw.Stop();
            double parallelForTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Parallel.For:        {parallelForTime:F2} ms ({iterations} iterations)");

            // Benchmark 3: PLINQ (Parallel LINQ)
            sw.Restart();
            for (int iter = 0; iter < iterations; iter++)
            {
                var samplesValidated = sampleGenerator.Samples
                    .AsParallel()
                    .Where((sample, index) =>
                        sample.ValidateSample(
                            index < sampleGenerator.Samples.Count - 1 ? sampleGenerator.Samples[index + 1] : null,
                            _sampleIncrement))
                    .Count();
            }
            sw.Stop();
            double plinqTime = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"PLINQ:               {plinqTime:F2} ms ({iterations} iterations)");

            // Calculate and display results
            Console.WriteLine($"\n--- Results ---");
            double[] times = { sequentialTime, parallelForTime, plinqTime };
            double fastest = times.Min();
            double slowest = times.Max();

            string[] methods = { "Sequential for loop", "Parallel.For", "PLINQ" };
            int fastestIdx = Array.IndexOf(times, fastest);
            int slowestIdx = Array.IndexOf(times, slowest);

            Console.WriteLine($"Fastest:  {methods[fastestIdx]} ({fastest:F2} ms)");
            Console.WriteLine($"Slowest:  {methods[slowestIdx]} ({slowest:F2} ms)");
            Console.WriteLine($"\nRelative Performance (vs Sequential):");
            Console.WriteLine($"  Sequential for loop: 1.00x (baseline)");
            Console.WriteLine($"  Parallel.For:        {parallelForTime / sequentialTime:F2}x");
            Console.WriteLine($"  PLINQ:               {plinqTime / sequentialTime:F2}x");
            Console.WriteLine($"\n==============================================\n");
        }
    }
}

