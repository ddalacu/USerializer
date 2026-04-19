``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 9V74, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.202
  [Host]   : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT
  ShortRun : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 211.2 μs |   5.11 μs |  0.28 μs |
| USerializerDeserialize | 282.2 μs | 121.33 μs |  6.65 μs |
|    MemoryPackSerialize | 220.2 μs | 324.26 μs | 17.77 μs |
|  MemoryPackDeserialize | 270.0 μs |   3.46 μs |  0.19 μs |
