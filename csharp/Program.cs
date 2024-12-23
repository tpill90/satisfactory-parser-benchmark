﻿global using System.Diagnostics;
global using SatisfactorySaveNet;
global using static Benchmark.SpectreFormatters;
global using Spectre.Console;
global using Console = Spectre.Console.AnsiConsole;

namespace Benchmark
{
    internal sealed class Program
    {
        private const string SaveFilePath = "../SampleSave.sav";
        private const int Iterations = 10;

        private static void Main()
        {
            Console.MarkupLine(Directory.GetCurrentDirectory());
            Console.MarkupLine(MediumPurple("Starting C# benchmark.."));
            Console.Console.Write(new Rule());

            // Loading the save file into memory once, so we don't want to include it in the benchmark
            var fileAsBytes = File.ReadAllBytes(SaveFilePath);

            RunWarmup();

            var runResults = new List<Stopwatch>();
            for (int i = 0; i < Iterations; i++)
            {
                // Force GC between runs, to minimize leftover objects from the previous run.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Run the actual code we want to benchmark
                var timer = Stopwatch.StartNew();
                SaveFileSerializer.Instance.Deserialize(fileAsBytes);

                // Save and log the results
                timer.Stop();
                runResults.Add(timer);
                Console.MarkupLine($"Run {Cyan(i + 1)} finished in {LightYellow(timer.ElapsedMilliseconds + "ms")}");
            }

            PrintResultsSummary(runResults);
        }

        // Runs a few iterations of the benchmarked code so that the JIT can do its thing.
        private static void RunWarmup()
        {
            var iterationCount = 10;
            var fileAsBytes = File.ReadAllBytes(SaveFilePath);

            var spectreProgress = Console.Progress().Columns(new List<ProgressColumn>
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new RemainingTimeColumn()
            }.ToArray());
            spectreProgress.Start(ctx =>
            {
                var overallProgressTask = ctx.AddTask("Running warmup", new ProgressTaskSettings { MaxValue = iterationCount });
                for (int i = 0; i < iterationCount; i++)
                {
                    SaveFileSerializer.Instance.Deserialize(fileAsBytes);
                    overallProgressTask.Increment(1);
                }
            });

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static void PrintResultsSummary(List<Stopwatch> runElapsedTimes)
        {
            // Calculating stats
            var fastestTime = runElapsedTimes.MinBy(e => e.Elapsed.TotalMilliseconds);
            var slowestTime = runElapsedTimes.MaxBy(e => e.Elapsed.TotalMilliseconds);
            var average = runElapsedTimes.Sum(e => e.Elapsed.TotalMilliseconds) / runElapsedTimes.Count;

            // White spacing + a horizontal rule to delineate that the command has completed
            Console.WriteLine();
            Console.Write(new Rule());

            // Building out summary table
            var summaryTable = new Table { Border = TableBorder.MinimalHeavyHead }
                               .AddColumn(new TableColumn(Cyan("Min")).Centered())
                               .AddColumn(new TableColumn(LightYellow("Average")).Centered())
                               .AddColumn(new TableColumn(MediumPurple("Max")).Centered())
                               .AddRow($"{fastestTime.ElapsedMilliseconds}ms", $"{(int)average}ms", $"{slowestTime.ElapsedMilliseconds}ms");

            // Setting up final formatting, to make sure padding and alignment is correct
            var grid = new Grid()
                       .AddColumn(new GridColumn())
                       // Summary Table
                       .AddRow(White(Underline("Benchmark summary")))
                       .AddRow(summaryTable)
                       .AddEmptyRow();

            Console.Write(grid);
        }
    }

    internal static class SpectreFormatters
    {
        public static string Cyan(object inputObj) => $"[rgb(97,200,214)]{inputObj}[/]";
        public static string Underline(object inputObj) => $"[underline]{inputObj}[/]";
        public static string LightYellow(object inputObj) => $"[rgb(249,241,165)]{inputObj}[/]";
        public static string MediumPurple(object inputObj) => $"[{Color.MediumPurple1.ToMarkup()}]{inputObj}[/]";
        public static string White(object inputObj) => $"[white]{inputObj}[/]";
    }
}
