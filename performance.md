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
|   USerializerSerialize | 217.6 μs |  77.32 μs |  4.24 μs |
| USerializerDeserialize | 310.2 μs |  37.56 μs |  2.06 μs |
|    MemoryPackSerialize | 253.0 μs | 869.34 μs | 47.65 μs |
|  MemoryPackDeserialize | 273.1 μs |  27.62 μs |  1.51 μs |
