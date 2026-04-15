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
|   USerializerSerialize | 242.1 μs |  1.60 μs | 0.09 μs |
| USerializerDeserialize | 297.6 μs | 26.83 μs | 1.47 μs |
|    MemoryPackSerialize | 295.3 μs | 47.13 μs | 2.58 μs |
|  MemoryPackDeserialize | 262.3 μs | 22.98 μs | 1.26 μs |
