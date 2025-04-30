``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=9.0.203
  [Host]   : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT
  ShortRun : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |    Error |    StdDev |
|----------------------- |----------:|---------:|----------:|
|   USerializerSerialize |  9.158 ms | 1.045 ms | 0.0573 ms |
| USerializerDeserialize | 26.306 ms | 3.422 ms | 0.1876 ms |
|    MemoryPackSerialize | 14.650 ms | 1.617 ms | 0.0886 ms |
|  MemoryPackDeserialize | 20.270 ms | 5.369 ms | 0.2943 ms |
|         CerasSerialize | 43.580 ms | 4.528 ms | 0.2482 ms |
|       CerasDeserialize | 49.517 ms | 7.881 ms | 0.4320 ms |
