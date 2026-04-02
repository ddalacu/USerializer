``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |   StdDev |
|----------------------- |---------:|---------:|---------:|
|   USerializerSerialize | 10.60 ms | 0.988 ms | 0.054 ms |
| USerializerDeserialize | 25.93 ms | 0.692 ms | 0.038 ms |
|    MemoryPackSerialize | 13.81 ms | 0.443 ms | 0.024 ms |
|  MemoryPackDeserialize | 20.25 ms | 0.352 ms | 0.019 ms |
|         CerasSerialize | 41.78 ms | 1.174 ms | 0.064 ms |
|       CerasDeserialize | 46.30 ms | 3.403 ms | 0.187 ms |
