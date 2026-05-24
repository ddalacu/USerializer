``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 9V74, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.300
  [Host]   : .NET Core 10.0.8 (CoreCLR 10.0.826.23019, CoreFX 10.0.826.23019), X64 RyuJIT
  ShortRun : .NET Core 10.0.8 (CoreCLR 10.0.826.23019, CoreFX 10.0.826.23019), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 515.2 μs |  18.25 μs |  1.00 μs |
| USerializerDeserialize | 628.3 μs | 171.27 μs |  9.39 μs |
|    MemoryPackSerialize | 566.1 μs | 540.01 μs | 29.60 μs |
|  MemoryPackDeserialize | 639.6 μs |  90.90 μs |  4.98 μs |
