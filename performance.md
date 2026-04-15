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
|   USerializerSerialize | 249.9 μs |  25.05 μs | 1.37 μs |
| USerializerDeserialize | 310.9 μs |  95.40 μs | 5.23 μs |
|    MemoryPackSerialize | 235.8 μs | 175.23 μs | 9.60 μs |
|  MemoryPackDeserialize | 258.5 μs |  17.01 μs | 0.93 μs |
