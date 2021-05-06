``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  8.354 ms | 0.0667 ms | 0.0037 ms |
| USerializerDeserialize | 24.735 ms | 3.2500 ms | 0.1781 ms |
|   MessagePackSerialize |  8.022 ms | 0.0215 ms | 0.0012 ms |
| MessagePackDeserialize | 24.679 ms | 6.9685 ms | 0.3820 ms |
|         CerasSerialize | 48.719 ms | 1.6541 ms | 0.0907 ms |
|       CerasDeserialize | 47.621 ms | 3.3666 ms | 0.1845 ms |
