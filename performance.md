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
|   USerializerSerialize | 10.81 ms | 1.745 ms | 0.096 ms |
| USerializerDeserialize | 26.49 ms | 2.281 ms | 0.125 ms |
|    MemoryPackSerialize | 13.90 ms | 0.162 ms | 0.009 ms |
|  MemoryPackDeserialize | 20.74 ms | 1.608 ms | 0.088 ms |
|         CerasSerialize | 42.03 ms | 0.471 ms | 0.026 ms |
|       CerasDeserialize | 45.84 ms | 0.562 ms | 0.031 ms |
