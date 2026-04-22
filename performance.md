``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 9V74, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.203
  [Host]   : .NET Core 10.0.7 (CoreCLR 10.0.726.21808, CoreFX 10.0.726.21808), X64 RyuJIT
  ShortRun : .NET Core 10.0.7 (CoreCLR 10.0.726.21808, CoreFX 10.0.726.21808), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |       Error |   StdDev |
|----------------------- |---------:|------------:|---------:|
|   USerializerSerialize | 529.4 μs |    17.30 μs |  0.95 μs |
| USerializerDeserialize | 736.4 μs |   239.08 μs | 13.10 μs |
|    MemoryPackSerialize | 531.8 μs | 1,770.52 μs | 97.05 μs |
|  MemoryPackDeserialize | 623.7 μs |    78.74 μs |  4.32 μs |
