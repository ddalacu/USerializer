``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.202
  [Host]   : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT
  ShortRun : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |  StdDev |
|----------------------- |---------:|----------:|--------:|
|   USerializerSerialize | 240.9 μs |   0.52 μs | 0.03 μs |
| USerializerDeserialize | 298.3 μs |  37.28 μs | 2.04 μs |
|    MemoryPackSerialize | 300.5 μs | 120.57 μs | 6.61 μs |
|  MemoryPackDeserialize | 258.4 μs |  21.53 μs | 1.18 μs |
