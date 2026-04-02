``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |   StdDev |
|----------------------- |---------:|---------:|---------:|
|   USerializerSerialize | 22.83 ms | 0.312 ms | 0.017 ms |
| USerializerDeserialize | 31.71 ms | 1.858 ms | 0.102 ms |
|    MemoryPackSerialize | 20.96 ms | 0.317 ms | 0.017 ms |
|  MemoryPackDeserialize | 26.27 ms | 2.297 ms | 0.126 ms |
|         CerasSerialize | 60.48 ms | 2.620 ms | 0.144 ms |
|       CerasDeserialize | 56.79 ms | 1.713 ms | 0.094 ms |
