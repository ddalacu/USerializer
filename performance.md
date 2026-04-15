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
|   USerializerSerialize | 245.0 μs |  16.38 μs |  0.90 μs |
| USerializerDeserialize | 303.9 μs |  35.00 μs |  1.92 μs |
|    MemoryPackSerialize | 223.3 μs | 218.28 μs | 11.96 μs |
|  MemoryPackDeserialize | 254.4 μs |  20.65 μs |  1.13 μs |
