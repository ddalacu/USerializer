``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.202
  [Host]   : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  ShortRun : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  8.392 ms |  2.090 ms | 0.1145 ms |
| USerializerDeserialize | 26.231 ms |  8.086 ms | 0.4432 ms |
|   MessagePackSerialize |  8.315 ms |  4.345 ms | 0.2382 ms |
| MessagePackDeserialize | 26.294 ms | 14.346 ms | 0.7864 ms |
|         CerasSerialize | 48.460 ms |  3.044 ms | 0.1668 ms |
|       CerasDeserialize | 46.317 ms | 15.815 ms | 0.8669 ms |
