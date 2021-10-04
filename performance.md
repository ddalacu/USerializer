``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.401
  [Host]   : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT
  ShortRun : .NET Core 5.0.10 (CoreCLR 5.0.1021.41214, CoreFX 5.0.1021.41214), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  6.603 ms |  3.419 ms | 0.1874 ms |
| USerializerDeserialize | 22.869 ms | 10.237 ms | 0.5611 ms |
|   MessagePackSerialize |  8.316 ms |  4.566 ms | 0.2503 ms |
| MessagePackDeserialize | 26.116 ms | 20.635 ms | 1.1311 ms |
|         CerasSerialize | 46.583 ms | 29.370 ms | 1.6099 ms |
|       CerasDeserialize | 49.519 ms | 22.897 ms | 1.2550 ms |
