``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.203
  [Host]   : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT
  ShortRun : .NET Core 5.0.6 (CoreCLR 5.0.621.22011, CoreFX 5.0.621.22011), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  8.391 ms |  1.6646 ms | 0.0912 ms |
| USerializerDeserialize | 25.696 ms |  3.5129 ms | 0.1926 ms |
|   MessagePackSerialize |  9.385 ms |  0.7866 ms | 0.0431 ms |
| MessagePackDeserialize | 30.838 ms | 12.7818 ms | 0.7006 ms |
|         CerasSerialize | 58.503 ms |  5.1733 ms | 0.2836 ms |
|       CerasDeserialize | 55.634 ms |  3.1500 ms | 0.1727 ms |
