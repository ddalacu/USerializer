``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.300
  [Host]   : .NET Core 10.0.8 (CoreCLR 10.0.826.23019, CoreFX 10.0.826.23019), X64 RyuJIT
  ShortRun : .NET Core 10.0.8 (CoreCLR 10.0.826.23019, CoreFX 10.0.826.23019), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 609.5 μs |  24.55 μs |  1.35 μs |
| USerializerDeserialize | 621.0 μs | 191.27 μs | 10.48 μs |
|    MemoryPackSerialize | 642.3 μs | 181.65 μs |  9.96 μs |
|  MemoryPackDeserialize | 626.6 μs | 102.67 μs |  5.63 μs |
