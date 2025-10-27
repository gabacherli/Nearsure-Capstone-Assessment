using System;
using System.Diagnostics;

namespace DevSample
{
    class Program
    {
        static readonly int _cyclesToRun;
        static readonly int _samplesToLoad;
        static readonly DateTime _sampleStartDate;
        static readonly TimeSpan _sampleIncrement;

        static Program()
        {
            //Note: these settings should not be modified
            _cyclesToRun = Environment.ProcessorCount > 1 ? Environment.ProcessorCount / 2 : 1; //hopefully we have more than 1 core to work with, run cores/2 cycles with a max of 4 cycles
            _cyclesToRun = _cyclesToRun > 4 ? 4 : _cyclesToRun;
            _samplesToLoad = 222222;
            _sampleStartDate = new DateTime(1990, 1, 1, 1, 1, 1, 1);
            _sampleIncrement = new TimeSpan(0, 5, 0);
        }

        static void Main(string[] args)
        {
            Stopwatch totalMonitor = new Stopwatch();
            totalMonitor.Start();

            LogMessage($"Starting Execution on a {Environment.ProcessorCount} core system. A total of {_cyclesToRun} cycles will be run");

            for (int i = 0; i < _cyclesToRun; i++)
            {
                try
                {

                    TimeSpan cycleElapsedTime = new TimeSpan();

                    Stopwatch cycleTimer = new Stopwatch();

                    SampleGenerator sampleGenerator = new SampleGenerator(_sampleStartDate, _sampleIncrement);

                    LogMessage($"Cycle {i} Started Sample Load.");

                    cycleTimer.Start();

                    sampleGenerator.LoadSamples(_samplesToLoad);

                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;

                    LogMessage($"Cycle {i} Finished Sample Load. Load Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");

                    LogMessage($"Cycle {i} Started Sample Validation.");

                    cycleTimer.Restart();

                    sampleGenerator.ValidateSamples();

                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;

                    LogMessage($"Cycle {i} Finished Sample Validation. Total Samples Validated: {sampleGenerator.SamplesValidated}. Validation Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");

                    float valueSum = 0;

                    foreach (Sample s in sampleGenerator.Samples)
                    {
                        valueSum += s.Value;
                    }

                    //TODO: why do we only seem to get 7 digits of precision? The CEO wants to see at least 20!
                    LogMessage($"Cycle {i} Sum of All Samples: {valueSum.ToString("N")}.");

                    LogMessage($"Cycle {i} Finished. Total Cycle Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Execution Failed!\n{ex.ToString()}");
                }
            }

            totalMonitor.Stop();

            LogMessage("-----");
            LogMessage($"Execution Finished. Total Elapsed Time: {totalMonitor.Elapsed.TotalMilliseconds.ToString("N")} ms.");

            Console.Read();
        }

        static void LogMessage(string message)
        {
            LogToFile(message);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fffff} - {message}");
        }

        static void LogToFile(string message)
        {
            //TODO: implement this when someone complains about it not working... 
            //everything written to the console should also be written to a log under C:\Temp. A new log with a unique file name should be created each time the application is run.

        }
    }
}
