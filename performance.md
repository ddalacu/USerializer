``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |  StdDev |
|----------------------- |---------:|---------:|--------:|
|   USerializerSerialize | 230.4 μs |  1.43 μs | 0.08 μs |
| USerializerDeserialize | 308.8 μs | 21.36 μs | 1.17 μs |
|    MemoryPackSerialize | 293.1 μs | 46.76 μs | 2.56 μs |
|  MemoryPackDeserialize | 259.5 μs | 10.92 μs | 0.60 μs |
