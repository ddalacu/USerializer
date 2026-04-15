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
|   USerializerSerialize | 213.8 μs |  15.77 μs |  0.86 μs |
| USerializerDeserialize | 324.7 μs | 165.64 μs |  9.08 μs |
|    MemoryPackSerialize | 220.5 μs | 485.22 μs | 26.60 μs |
|  MemoryPackDeserialize | 281.8 μs |   5.60 μs |  0.31 μs |
