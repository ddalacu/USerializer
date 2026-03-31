``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 9V74, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |   StdDev |
|----------------------- |---------:|---------:|---------:|
|   USerializerSerialize | 11.08 ms | 0.331 ms | 0.018 ms |
| USerializerDeserialize | 26.90 ms | 5.913 ms | 0.324 ms |
|    MemoryPackSerialize | 12.46 ms | 0.601 ms | 0.033 ms |
|  MemoryPackDeserialize | 21.13 ms | 1.725 ms | 0.095 ms |
|         CerasSerialize | 40.30 ms | 9.003 ms | 0.494 ms |
|       CerasDeserialize | 40.25 ms | 2.377 ms | 0.130 ms |
