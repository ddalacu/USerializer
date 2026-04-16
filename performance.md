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
|   USerializerSerialize | 240.0 μs |  48.50 μs |  2.66 μs |
| USerializerDeserialize | 308.1 μs |  66.10 μs |  3.62 μs |
|    MemoryPackSerialize | 238.5 μs | 452.85 μs | 24.82 μs |
|  MemoryPackDeserialize | 261.9 μs |  82.99 μs |  4.55 μs |
