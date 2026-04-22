``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.203
  [Host]   : .NET Core 10.0.7 (CoreCLR 10.0.726.21808, CoreFX 10.0.726.21808), X64 RyuJIT
  ShortRun : .NET Core 10.0.7 (CoreCLR 10.0.726.21808, CoreFX 10.0.726.21808), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 586.5 μs |  18.09 μs |  0.99 μs |
| USerializerDeserialize | 811.2 μs |  30.26 μs |  1.66 μs |
|    MemoryPackSerialize | 667.1 μs | 285.80 μs | 15.67 μs |
|  MemoryPackDeserialize | 644.6 μs |  12.45 μs |  0.68 μs |
