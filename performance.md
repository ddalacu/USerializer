``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 232.6 μs |  10.25 μs |  0.56 μs |
| USerializerDeserialize | 321.3 μs |  70.65 μs |  3.87 μs |
|    MemoryPackSerialize | 266.7 μs | 306.70 μs | 16.81 μs |
|  MemoryPackDeserialize | 265.2 μs |   4.57 μs |  0.25 μs |
