``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.400
  [Host]   : .NET Core 10.0.11 (CoreCLR 10.0.1126.37416, CoreFX 10.0.1126.37416), X64 RyuJIT
  ShortRun : .NET Core 10.0.11 (CoreCLR 10.0.1126.37416, CoreFX 10.0.1126.37416), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |       Error |   StdDev |
|----------------------- |---------:|------------:|---------:|
|   USerializerSerialize | 590.5 μs |     9.79 μs |  0.54 μs |
| USerializerDeserialize | 630.1 μs |    28.95 μs |  1.59 μs |
|    MemoryPackSerialize | 594.3 μs | 1,118.90 μs | 61.33 μs |
|  MemoryPackDeserialize | 661.2 μs |    41.01 μs |  2.25 μs |
