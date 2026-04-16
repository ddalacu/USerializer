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
|   USerializerSerialize | 243.9 μs |   2.87 μs | 0.16 μs |
| USerializerDeserialize | 325.1 μs |  31.33 μs | 1.72 μs |
|    MemoryPackSerialize | 216.9 μs |  69.43 μs | 3.81 μs |
|  MemoryPackDeserialize | 273.8 μs | 103.79 μs | 5.69 μs |
