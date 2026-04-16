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
|   USerializerSerialize | 248.2 μs |  79.19 μs |  4.34 μs |
| USerializerDeserialize | 286.2 μs |  25.63 μs |  1.40 μs |
|    MemoryPackSerialize | 233.0 μs | 459.73 μs | 25.20 μs |
|  MemoryPackDeserialize | 267.0 μs |  24.73 μs |  1.36 μs |
