using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DevSample
{
    class Program
    {
        static readonly int _cyclesToRun;
        static readonly int _samplesToLoad;
        static readonly DateTime _sampleStartDate;
        static readonly TimeSpan _sampleIncrement;
        static readonly string _logFilePath;
        static readonly StringBuilder _logBuffer = new StringBuilder();

        static Program()
        {
            //Note: these settings should not be modified
            _cyclesToRun = Environment.ProcessorCount > 1 ? Environment.ProcessorCount / 2 : 1; //hopefully we have more than 1 core to work with, run cores/2 cycles with a max of 4 cycles
            _cyclesToRun = _cyclesToRun > 4 ? 4 : _cyclesToRun;
            _samplesToLoad = 222222;
            _sampleStartDate = new DateTime(1990, 1, 1, 1, 1, 1, 1);
            _sampleIncrement = new TimeSpan(0, 5, 0);

            bool isLocal = bool.TryParse(ConfigurationManager.AppSettings["IS_LOCAL"], out bool result) && result;
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var logDirectory = isLocal ? Path.Combine("../../temp") : @"C:\temp";

            _logFilePath = Path.Combine(logDirectory, $"Log_{timestamp}.txt");

            Directory.CreateDirectory(logDirectory);
        }

        static void Main()
        {
            Stopwatch totalMonitor = new Stopwatch();
            totalMonitor.Start();

            LogMessage("Starting Execution on a {0} core system. A total of {1} cycles will be run", Environment.ProcessorCount, _cyclesToRun);

            for (int i = 0; i < _cyclesToRun; i++)
            {
                try
                {

                    TimeSpan cycleElapsedTime = new TimeSpan();

                    Stopwatch cycleTimer = new Stopwatch();

                    SampleGenerator sampleGenerator = new SampleGenerator(_sampleStartDate, _sampleIncrement);

                    LogMessage("Cycle {0} Started Sample Load.", i);

                    cycleTimer.Start();

                    sampleGenerator.LoadSamples(_samplesToLoad);

                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;

                    LogMessage("Cycle {0} Finished Sample Load. Load Time: {1:N} ms.", i, cycleElapsedTime.TotalMilliseconds);

                    LogMessage("Cycle {0} Started Sample Validation.", i);

                    cycleTimer.Restart();

                    sampleGenerator.ValidateSamples();

                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;

                    LogMessage("Cycle {0} Finished Sample Validation. Total Samples Validated: {1}. Validation Time: {2:N} ms.", i, sampleGenerator.SamplesValidated, cycleElapsedTime.TotalMilliseconds);

                    float valueSum = 0;

                    foreach (Sample s in sampleGenerator.Samples)
                    {
                        valueSum += s.Value;
                    }

                    LogMessage("Cycle {0} Sum of All Samples: {1:N}.", i, valueSum);

                    LogMessage("Cycle {0} Finished. Total Cycle Time: {1:N} ms.", i, cycleElapsedTime.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    LogMessage("Execution Failed!\n{0}", ex.ToString());
                }
            }

            totalMonitor.Stop();

            LogMessage("-----");
            LogMessage("Execution Finished. Total Elapsed Time: {0:N} ms.", totalMonitor.Elapsed.TotalMilliseconds);

            FlushBufferToLogFile();

            Console.Read();
        }

        /// <summary>
        /// Logs a message to both the console and the internal log buffer.
        /// </summary>
        /// <param name="message">The message to log, optionally containing parameter placeholders.</param>
        /// <param name="args">Optional arguments to format into the message. Also useful in case we need structured logging in the future.</param>
        static void LogMessage(string message, params object[] args)
        {
            string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;

            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fffff} - {formattedMessage}");
            _logBuffer.AppendLine(formattedMessage);
        }

        /// <summary>
        /// Flushes all buffered log messages to the log file.
        /// </summary>
        static void FlushBufferToLogFile()
        {
            if (_logBuffer.Length > 0)
            {
                string output = _logBuffer.ToString();

                try
                {
                    File.WriteAllText(_logFilePath, output);
                }
                catch (Exception ex)
        {
                    Console.WriteLine($"Failed to write logs to file: {ex.Message}");
                }

                _logBuffer.Clear();
            }
        }
    }
}
