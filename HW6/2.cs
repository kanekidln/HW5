using System;
using System.Diagnostics;
using System.Text;

namespace DelegatesLinQ.Homework
{
    public delegate string DataProcessor(string input);
    public delegate void ProcessingEventHandler(string stage, string input, string output);

    public class DataProcessingPipeline
    {
        public event ProcessingEventHandler ProcessingStageCompleted;

        public static string RemoveSpaces(string input)
        {
            string output = input.Replace(" ", "");
            return output;
        }

        public static string ToUpperCase(string input)
        {
            string output = input.ToUpper();
            return output;
        }

        public static string AddTimestamp(string input)
        {
            string output = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {input}";
            return output;
        }

        public static string ReverseString(string input)
        {
            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string EncodeBase64(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        public static string ValidateInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or empty");
            return input;
        }

        public string ProcessData(string input, DataProcessor pipeline)
        {
            string currentInput = input;
            foreach (Delegate d in pipeline.GetInvocationList())
            {
                var processor = (DataProcessor)d;
                string stageName = processor.Method.Name;
                Stopwatch sw = Stopwatch.StartNew();
                string output;
                try
                {
                    output = processor(currentInput);
                }
                catch (Exception ex)
                {
                    output = $"[Error: {ex.Message}]";
                }
                sw.Stop();

                // Raise event
                OnProcessingStageCompleted(stageName, currentInput, output);
                currentInput = output;
            }
            return currentInput;
        }

        protected virtual void OnProcessingStageCompleted(string stage, string input, string output)
        {
            ProcessingStageCompleted?.Invoke(stage, input, output);
        }
    }

    public class ProcessingLogger
    {
        public void OnProcessingStageCompleted(string stage, string input, string output)
        {
            Console.WriteLine($"[LOG] Stage: {stage}, Input: \"{input}\", Output: \"{output}\"");
        }
    }

    public class PerformanceMonitor
    {
        private int _stageCount = 0;

        public void OnProcessingStageCompleted(string stage, string input, string output)
        {
            _stageCount++;
        }

        public void DisplayStatistics()
        {
            Console.WriteLine($"\n[STATS] Total stages executed: {_stageCount}");
        }
    }

    public class DelegateChain
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== HOMEWORK 2: CUSTOM DELEGATE CHAIN ===\n");

            DataProcessingPipeline pipeline = new DataProcessingPipeline();
            ProcessingLogger logger = new ProcessingLogger();
            PerformanceMonitor monitor = new PerformanceMonitor();

            pipeline.ProcessingStageCompleted += logger.OnProcessingStageCompleted;
            pipeline.ProcessingStageCompleted += monitor.OnProcessingStageCompleted;

            DataProcessor chain = DataProcessingPipeline.ValidateInput;
            chain += DataProcessingPipeline.RemoveSpaces;
            chain += DataProcessingPipeline.ToUpperCase;
            chain += DataProcessingPipeline.AddTimestamp;

            string input = "Hello World from C#";
            Console.WriteLine($"Input: {input}");
            string result = pipeline.ProcessData(input, chain);
            Console.WriteLine($"Output: {result}\n");

            // Add more processors
            chain += DataProcessingPipeline.ReverseString;
            chain += DataProcessingPipeline.EncodeBase64;

            result = pipeline.ProcessData("Extended Pipeline Test", chain);
            Console.WriteLine($"Extended Output: {result}\n");

            // Remove a processor
            chain -= DataProcessingPipeline.ReverseString;
            result = pipeline.ProcessData("Without Reverse", chain);
            Console.WriteLine($"Modified Output: {result}");

            // Display stats
            monitor.DisplayStatistics();

            // Error test
            try
            {
                result = pipeline.ProcessData(null, chain);
                Console.WriteLine($"Null Input Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handled: {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}
