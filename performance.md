``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.202
  [Host]   : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT
  ShortRun : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 207.9 μs |  12.67 μs |  0.69 μs |
| USerializerDeserialize | 306.5 μs |  10.05 μs |  0.55 μs |
|    MemoryPackSerialize | 220.9 μs | 567.89 μs | 31.13 μs |
|  MemoryPackDeserialize | 261.5 μs |  54.04 μs |  2.96 μs |
