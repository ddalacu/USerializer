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
|   USerializerSerialize | 229.3 μs |  14.69 μs |  0.81 μs |
| USerializerDeserialize | 331.8 μs |  24.54 μs |  1.35 μs |
|    MemoryPackSerialize | 280.3 μs | 481.04 μs | 26.37 μs |
|  MemoryPackDeserialize | 269.6 μs |  38.65 μs |  2.12 μs |
