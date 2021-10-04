``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.401
  [Host]   : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT
  ShortRun : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  6.136 ms |  2.626 ms | 0.1440 ms |
| USerializerDeserialize | 25.148 ms |  5.700 ms | 0.3124 ms |
|   MessagePackSerialize |  8.279 ms |  3.389 ms | 0.1857 ms |
| MessagePackDeserialize | 24.669 ms |  6.518 ms | 0.3573 ms |
|         CerasSerialize | 46.394 ms |  8.586 ms | 0.4706 ms |
|       CerasDeserialize | 46.130 ms | 48.538 ms | 2.6605 ms |
