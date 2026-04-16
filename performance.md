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
|   USerializerSerialize | 241.9 μs |  36.19 μs |  1.98 μs |
| USerializerDeserialize | 307.7 μs |  51.36 μs |  2.82 μs |
|    MemoryPackSerialize | 226.9 μs | 295.12 μs | 16.18 μs |
|  MemoryPackDeserialize | 260.9 μs |  35.08 μs |  1.92 μs |
