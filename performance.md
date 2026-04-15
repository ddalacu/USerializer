``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 9V74, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.202
  [Host]   : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT
  ShortRun : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |  StdDev |
|----------------------- |---------:|----------:|--------:|
|   USerializerSerialize | 225.1 μs |   2.96 μs | 0.16 μs |
| USerializerDeserialize | 309.7 μs |  13.29 μs | 0.73 μs |
|    MemoryPackSerialize | 243.6 μs | 176.25 μs | 9.66 μs |
|  MemoryPackDeserialize | 270.8 μs |  18.15 μs | 1.00 μs |
