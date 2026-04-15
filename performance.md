``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.202
  [Host]   : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT
  ShortRun : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |  StdDev |
|----------------------- |---------:|---------:|--------:|
|   USerializerSerialize | 248.6 μs |  6.49 μs | 0.36 μs |
| USerializerDeserialize | 319.2 μs | 45.21 μs | 2.48 μs |
|    MemoryPackSerialize | 222.1 μs | 44.66 μs | 2.45 μs |
|  MemoryPackDeserialize | 276.1 μs |  7.61 μs | 0.42 μs |
