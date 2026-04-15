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
|   USerializerSerialize | 247.8 μs |   3.52 μs |  0.19 μs |
| USerializerDeserialize | 314.5 μs |  13.78 μs |  0.76 μs |
|    MemoryPackSerialize | 227.9 μs | 196.33 μs | 10.76 μs |
|  MemoryPackDeserialize | 260.7 μs |  19.50 μs |  1.07 μs |
