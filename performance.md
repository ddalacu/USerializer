``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize | 10.571 ms |  2.0719 ms | 0.1136 ms |
| USerializerDeserialize | 32.070 ms |  4.1953 ms | 0.2300 ms |
|   MessagePackSerialize |  9.453 ms |  0.0745 ms | 0.0041 ms |
| MessagePackDeserialize | 32.260 ms |  9.8233 ms | 0.5384 ms |
|         CerasSerialize | 58.022 ms | 10.7485 ms | 0.5892 ms |
|       CerasDeserialize | 59.334 ms | 10.7820 ms | 0.5910 ms |
