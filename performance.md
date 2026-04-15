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
|   USerializerSerialize | 229.8 μs |   9.95 μs |  0.55 μs |
| USerializerDeserialize | 320.2 μs |  17.51 μs |  0.96 μs |
|    MemoryPackSerialize | 273.1 μs | 392.29 μs | 21.50 μs |
|  MemoryPackDeserialize | 270.2 μs |  66.67 μs |  3.65 μs |
