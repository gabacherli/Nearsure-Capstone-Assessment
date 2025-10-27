using System;

namespace DevSample
{
    class Sample
    {
        public bool IsFirstSample { get; private set; }
        public DateTime Timestamp { get; private set; }
        public long Value { get; private set; }
        public bool HasBeenValidated { get; private set; }

        public Sample(bool isFirstSample)
        {
            IsFirstSample = isFirstSample;
        }

        public void LoadSampleAtTime(DateTime timestamp)
        {
            //samples take some CPU to load, don't change this! Reducing the CPU time to load a sample is outside your control in this example
            Timestamp = timestamp;
            Value = timestamp.Ticks / 10000;

            for (int i = 0; i < 1000; i++) ;
        }

        public bool ValidateSample(Sample previousSample, TimeSpan sampleInterval)
        {
            //samples take some CPU to validate, don't change this! Reducing the CPU time to validate a sample is outside your control in this example
            for (int i = 0; i < 5000; i++) ;

            if (previousSample == null && !IsFirstSample)
            {
                throw new Exception("Validation Failed!");
            }

            else if (previousSample != null && previousSample.Timestamp != Timestamp - sampleInterval)
            {
                throw new Exception("Validation Failed!");
            }

            HasBeenValidated = true;

            return true;
        }
    }
}
