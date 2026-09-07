``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.400
  [Host]   : .NET Core 10.0.11 (CoreCLR 10.0.1126.37416, CoreFX 10.0.1126.37416), X64 RyuJIT
  ShortRun : .NET Core 10.0.11 (CoreCLR 10.0.1126.37416, CoreFX 10.0.1126.37416), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |  StdDev |
|----------------------- |---------:|----------:|--------:|
|   USerializerSerialize | 606.6 μs | 130.76 μs | 7.17 μs |
| USerializerDeserialize | 600.6 μs |  69.78 μs | 3.82 μs |
|    MemoryPackSerialize | 672.9 μs |  31.88 μs | 1.75 μs |
|  MemoryPackDeserialize | 648.8 μs |  81.74 μs | 4.48 μs |
