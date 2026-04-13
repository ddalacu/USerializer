``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 228.7 μs |   1.99 μs |  0.11 μs |
| USerializerDeserialize | 305.6 μs |  79.02 μs |  4.33 μs |
|    MemoryPackSerialize | 261.2 μs | 765.66 μs | 41.97 μs |
|  MemoryPackDeserialize | 260.0 μs |  13.98 μs |  0.77 μs |
