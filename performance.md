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
|   USerializerSerialize | 10.63 ms | 0.428 ms | 0.023 ms |
| USerializerDeserialize | 24.02 ms | 0.484 ms | 0.027 ms |
|    MemoryPackSerialize | 13.94 ms | 0.211 ms | 0.012 ms |
|  MemoryPackDeserialize | 20.39 ms | 1.930 ms | 0.106 ms |
|         CerasSerialize | 42.13 ms | 2.552 ms | 0.140 ms |
|       CerasDeserialize | 48.39 ms | 2.492 ms | 0.137 ms |
