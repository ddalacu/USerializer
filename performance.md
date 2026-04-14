``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 9V74, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 199.9 μs |   1.49 μs |  0.08 μs |
| USerializerDeserialize | 297.5 μs |  12.78 μs |  0.70 μs |
|    MemoryPackSerialize | 218.9 μs | 409.66 μs | 22.46 μs |
|  MemoryPackDeserialize | 274.4 μs | 213.76 μs | 11.72 μs |
