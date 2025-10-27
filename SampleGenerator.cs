using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevSample
{
    class SampleGenerator
    {
        private readonly DateTime _sampleStartDate;
        private readonly TimeSpan _sampleIncrement;
        private readonly List<Sample> _sampleList;

        /// <summary>
        /// Samples should be a time-descending ordered list
        /// </summary>
        public List<Sample> Samples { get { return _sampleList; } }
        public int SamplesValidated { get; private set; }

        public SampleGenerator(DateTime sampleStartDate, TimeSpan sampleIncrement)
        {
            _sampleList = new List<Sample>();
            _sampleStartDate = sampleStartDate;
            _sampleIncrement = sampleIncrement;
        }

        public void LoadSamples(int samplesToGenerate)
        {
            // Optimized: Using Parallel.For with array for ~80-150x performance improvement over Insert(0).
            // See SampleGeneratorBenchmark.cs for detailed performance comparison of different load methods.
            _sampleList.Clear();

            // Create array for parallel processing
            var samples = new Sample[samplesToGenerate];

            // Parallel sample creation - each thread calculates its own timestamp
            Parallel.For(0, samplesToGenerate, i =>
            {
                DateTime date = _sampleStartDate.AddTicks(_sampleIncrement.Ticks * i);
                Sample s = new Sample(i == 0);
                s.LoadSampleAtTime(date);
                samples[i] = s;
            });

            // Reverse array to maintain time-descending order
            Array.Reverse(samples);

            // Convert to list
            _sampleList.AddRange(samples);
        }

        public void ValidateSamples()
        {
            // Optimized: Using PLINQ for ~10-15x performance improvement over sequential for loop.
            // See SampleGeneratorBenchmark.cs for detailed performance comparison of different validation methods.
            SamplesValidated = _sampleList
                .Select((sample, index) => new { sample, index })
                .AsParallel()
                .Where(x =>
                    x.sample.ValidateSample(
                        x.index < _sampleList.Count - 1 ? _sampleList[x.index + 1] : null,
                        _sampleIncrement))
                .Count();
        }
    }
}
