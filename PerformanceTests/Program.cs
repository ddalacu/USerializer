using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using CommandLine;
using Microcharts;
using SkiaSharp;

namespace PerformanceTests
{



    class Program
    {
        public class Options
        {
            [Option('b', "branch", Required = false, HelpText = "Branch.", Default = "UNKNOWN-BRANCH")]
            public string Branch { get; set; }
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
        static double ConvertNanosecondsToMilliseconds(double nanoseconds)
        {
            return nanoseconds * 0.000001;
        }

        private static void Execute(Options options)
        {
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

        private static void WriteImage(Options options, Summary summary, string file)
        {
            var chartEntries = new List<ChartEntry>();

            double maxValue = 0;

            foreach (var benchmarkReport in summary.Reports)
            {
                var meanMs = ConvertNanosecondsToMilliseconds(benchmarkReport.ResultStatistics.Mean);


                var methodDisplayInfo = benchmarkReport.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo;

                SKColor color = default;

                if (methodDisplayInfo.Contains("Deserialize", StringComparison.OrdinalIgnoreCase))
                    color = SKColor.Parse("#ff944d");
                else
                    color = SKColor.Parse("#6666ff");

                chartEntries.Add(new ChartEntry((float)meanMs)
                {
                    ValueLabelColor = new SKColor(10, 10, 10),
                    TextColor = new SKColor(1, 10, 10),
                    Color = color,
                    Label = methodDisplayInfo,
                    ValueLabel = meanMs.ToString("#.##  ms")
                });

                if (meanMs > maxValue)
                    maxValue = meanMs;
            }


            var chart = new BarChart
            {
                Entries = chartEntries,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                IsAnimated = false,
                AnimationProgress = 1,
                LabelTextSize = 14,
                Typeface = SKTypeface.Default,
                MaxValue = (float) (maxValue * 1.2f),
            };

            var width = 1400;
            var height = 700;

            var info = new SKImageInfo(width, height, SKColorType.Rgba8888);

            using (var surface = SKSurface.Create(info))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);


                //canvas.DrawColor(new SKColor(0, 0, 0), SKBlendMode.Src);

                canvas.Translate(0,100);
                chart.Draw(canvas, width, height);
                canvas.Translate(0, -100);

                //canvas.Save();


                var paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(0x2c, 0x3e, 0x50),
                    StrokeCap = SKStrokeCap.Round,
                    TextSize = 20
                };

                canvas.DrawText($"{DateTime.Now.ToLongDateString()}  {options.Branch}", new SKPoint(50, 25), paint);
                //canvas.DrawText(DateTime.Now.ToLongDateString(), new SKPoint(50, 100), paint);

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
