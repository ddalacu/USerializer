``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.403
  [Host]   : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT
  ShortRun : .NET Core 5.0.12 (CoreCLR 5.0.1221.52207, CoreFX 5.0.1221.52207), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  9.613 ms |  6.373 ms | 0.3493 ms |
| USerializerDeserialize | 27.868 ms | 30.839 ms | 1.6904 ms |
|   MessagePackSerialize |  8.695 ms |  1.553 ms | 0.0851 ms |
| MessagePackDeserialize | 29.254 ms |  6.025 ms | 0.3303 ms |
|         CerasSerialize | 56.154 ms | 58.149 ms | 3.1873 ms |
|       CerasDeserialize | 54.067 ms | 23.695 ms | 1.2988 ms |
