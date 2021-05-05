using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using CommandLine;
using Microcharts;
using SkiaSharp;


[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

namespace PerformanceTests
{

    class Program
    {
        public class Options
        {
            [Option('c', "commit", Required = false, HelpText = "Commit.", Default = "UNKNOWN-ID")]
            public string Commit { get; set; }
        }


        static int Main(string[] args)
        {
            var fail = 0;
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(Execute).WithNotParsed(
                delegate (IEnumerable<Error> errors)
                {
                    foreach (var error in errors)
                    {
                        Console.Error.WriteLine(error.Tag);
                    }

                    fail = 1;
                });
            return fail;
        }

        private static void Execute(Options options)
        {
            //var serializer = new USerializer(new UnitySerializationPolicy(), ProvidersUtils.GetDefaultProviders(),
            //    new DataTypesDatabase());
            //serializer.PreCacheType(typeof(BookShelf));

            var summary = BenchmarkRunner.Run<SerializationBenchmarks>();

            var directory = "public";
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
            var file = Path.Combine(directory, $"output.png");


            var htmlResult = Directory.GetFiles(summary.ResultsDirectoryPath, "*.md").FirstOrDefault();

            if (htmlResult != null)
            {
                var destFileName = Path.Combine(directory, $"performance.md");

                if (File.Exists(destFileName))
                    File.Delete(destFileName);

                File.Move(htmlResult, destFileName);
            }

            WriteImage(options, summary, file);
        }

        private static double ConvertNanosecondsToMilliseconds(double nanoseconds)
        {
            return nanoseconds * 0.000001;
        }

        private static void WriteImage(Options options, Summary summary, string file)
        {
            var chartEntries = new List<ChartEntry>();

            double maxValue = 0;

            foreach (var benchmarkReport in summary.Reports)
            {
                var meanMs = ConvertNanosecondsToMilliseconds(benchmarkReport.ResultStatistics.Mean);

                if (meanMs > maxValue)
                    maxValue = meanMs;
            }


            foreach (var benchmarkReport in summary.Reports)
            {
                var meanMs = ConvertNanosecondsToMilliseconds(benchmarkReport.ResultStatistics.Mean);

                var methodDisplayInfo = benchmarkReport.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo;

                SKColor color;

                if (methodDisplayInfo.Contains("Deserialize", StringComparison.OrdinalIgnoreCase))
                    color = SKColor.Parse("#ff944d");
                else
                    color = SKColor.Parse("#6666ff");

                var value = (float)(meanMs / maxValue);
                chartEntries.Add(new ChartEntry(value)
                {
                    ValueLabelColor = new SKColor(10, 10, 10),
                    TextColor = new SKColor(1, 10, 10),
                    Color = color,
                    Label = methodDisplayInfo,
                    ValueLabel = meanMs.ToString("#.## ms")
                });
            }

            var chart = new BarChart
            {
                Entries = chartEntries,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                LabelTextSize = 14,
                Typeface = SKTypeface.Default,
                MaxValue = 1.15f,
                AnimationDuration = TimeSpan.Zero,
            };
            
            chart.IsAnimated = true;//seems stupid but there is some strange bug when building on git actions where it tries to animate so do not remove this
            chart.IsAnimated = false;//seems stupid but there is some strange bug when building on git actions where it tries to animate so do not remove this

            var width = 1400;

            var height = 700;

            var info = new SKImageInfo(width, height, SKColorType.Rgba8888);

            using (var surface = SKSurface.Create(info))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                canvas.Translate(0, 100);

                chart.DrawContent(canvas, width, height - 100);
                canvas.Translate(0, -100);

                var paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(0x2c, 0x3e, 0x50),
                    StrokeCap = SKStrokeCap.Round,
                    TextSize = 20
                };

                canvas.DrawText($"{DateTime.Now.ToLongDateString()}  {options.Commit}", new SKPoint(50, 25), paint);

                canvas.Flush();

                canvas.Save();

                var snapshot = surface.Snapshot();

                using (var data = snapshot.Encode(SKEncodedImageFormat.Png, 80))
                {
                    using (var stream = File.OpenWrite(file))
                    {
                        data.SaveTo(stream);
                    }
                }
            }
        }
    }
}
