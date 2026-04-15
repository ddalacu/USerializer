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
|   USerializerSerialize | 244.0 μs |   6.78 μs | 0.37 μs |
| USerializerDeserialize | 300.8 μs |  28.92 μs | 1.58 μs |
|    MemoryPackSerialize | 222.3 μs | 179.84 μs | 9.86 μs |
|  MemoryPackDeserialize | 259.7 μs |  10.14 μs | 0.56 μs |
