using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.CommandLineUtils;
using System.Threading;

namespace PSDocs.Benchmark
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "PSDocs Benchmark",
                Description = "A runner for testing PSDocs performance"
            };

#if !BENCHMARK
            // Do profiling
            DebugProfile();
#endif

#if BENCHMARK
            RunProfile(app);
            app.Execute(args);
#endif
        }

        private static void RunProfile(CommandLineApplication app)
        {
            var config = ManualConfig.CreateEmpty()
                .AddLogger(ConsoleLogger.Default)
                .AddColumnProvider(DefaultColumnProviders.Instance)
                .AddAnalyser(EnvironmentAnalyser.Default)
                .AddAnalyser(OutliersAnalyser.Default)
                .AddAnalyser(MinIterationTimeAnalyser.Default)
                .AddAnalyser(MultimodalDistributionAnalyzer.Default)
                .AddAnalyser(RuntimeErrorAnalyser.Default)
                .AddAnalyser(ZeroMeasurementAnalyser.Default);

            app.Command("benchmark", cmd =>
            {
                var output = cmd.Option("-o | --output", "The path to store report output.", CommandOptionType.SingleValue);

                cmd.OnExecute(() =>
                {
                    if (output.HasValue())
                    {
                        config.WithArtifactsPath(output.Value());
                    }

                    // Do benchmarks
                    BenchmarkRunner.Run<PSDocs>(config);

                    return 0;
                });

                cmd.HelpOption("-? | -h | --help");
            });

            app.HelpOption("-? | -h | --help");
        }

        private static void DebugProfile()
        {
            Thread.Sleep(2000);
            var profile = new PSDocs();
            profile.Prepare();

            for (var i = 0; i < 1000; i++)
            {
                profile.InvokeMarkdownProcessor();
            }
        }
    }
}
