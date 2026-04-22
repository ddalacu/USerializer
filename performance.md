``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 9V74, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.203
  [Host]   : .NET Core 10.0.7 (CoreCLR 10.0.726.21808, CoreFX 10.0.726.21808), X64 RyuJIT
  ShortRun : .NET Core 10.0.7 (CoreCLR 10.0.726.21808, CoreFX 10.0.726.21808), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 494.6 μs |  11.68 μs |  0.64 μs |
| USerializerDeserialize | 734.6 μs | 183.75 μs | 10.07 μs |
|    MemoryPackSerialize | 478.1 μs |  80.56 μs |  4.42 μs |
|  MemoryPackDeserialize | 632.3 μs |  33.02 μs |  1.81 μs |
