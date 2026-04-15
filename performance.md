``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.202
  [Host]   : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT
  ShortRun : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 244.6 μs |   7.14 μs |  0.39 μs |
| USerializerDeserialize | 308.7 μs |  48.47 μs |  2.66 μs |
|    MemoryPackSerialize | 259.1 μs | 494.55 μs | 27.11 μs |
|  MemoryPackDeserialize | 260.0 μs |  51.94 μs |  2.85 μs |
