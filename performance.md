``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.202
  [Host]   : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT
  ShortRun : .NET Core 10.0.6 (CoreCLR 10.0.626.17701, CoreFX 10.0.626.17701), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 239.9 μs |   1.53 μs |  0.08 μs |
| USerializerDeserialize | 302.6 μs |  29.06 μs |  1.59 μs |
|    MemoryPackSerialize | 252.0 μs | 303.80 μs | 16.65 μs |
|  MemoryPackDeserialize | 262.3 μs |  44.78 μs |  2.45 μs |
