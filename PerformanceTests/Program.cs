﻿using System;
using System.Collections.Generic;
using System.IO;
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
                delegate(IEnumerable<Error> errors)
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
            var chart = new BarChart
            {
                Entries = new List<ChartEntry>()
                {
                    new ChartEntry(10)
                    {
                        ValueLabelColor = new SKColor(255,0,255,255),
                        TextColor  = new SKColor(1,255,255,255),
                        Color  = new SKColor(1,1,255,255),
                        Label = "USerializer",
                        ValueLabel = "11"
                    },
                    new ChartEntry(88)
                    {
                        ValueLabelColor = new SKColor(255,0,255,255),
                        TextColor  = new SKColor(1,255,255,255),
                        Color  = new SKColor(1,1,255,255),
                        Label = "USerializer",
                        ValueLabel = "123"
                    },
                },
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal
            };

            chart.MaxValue = 100;
            chart.IsAnimated = false;
            //chart.BackgroundColor = new SKColor(255, 255, 255);

            // chart.LabelTextSize = 10;

            var info = new SKImageInfo(1024, 1024, SKColorType.Rgba8888);

            using (var surface = SKSurface.Create(info))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                //canvas.DrawColor(new SKColor(0, 0, 0), SKBlendMode.Src);
                chart.AnimationProgress = 1;
                chart.DrawContent(canvas, 1024, 1024);

                //canvas.Save();


                var paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(0x2c, 0x3e, 0x50),
                    StrokeCap = SKStrokeCap.Round,
                    TextSize = 20
                };
                Console.WriteLine(options.Branch);
                canvas.DrawText($"Branch:{options.Branch}", new SKPoint(50, 50), paint);

                canvas.Flush();
                canvas.Save();


                var snapshot = surface.Snapshot();

                var directory = "public";

                if (Directory.Exists(directory) == false)
                    Directory.CreateDirectory(directory);

                var file = Path.Combine(directory, $"output.png");

                using (var data = snapshot.Encode(SKEncodedImageFormat.Png, 80))
                {
                    using (var stream = File.OpenWrite(file))
                    {
                        data.SaveTo(stream);
                    }
                }

                File.WriteAllText(Path.Combine(directory, "index.html"), "Nothing to see here, go to the repo page!");
            }
        }
    }
}
