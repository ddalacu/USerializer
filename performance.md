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
|   USerializerSerialize | 10.52 ms | 0.095 ms | 0.005 ms |
| USerializerDeserialize | 26.01 ms | 4.000 ms | 0.219 ms |
|    MemoryPackSerialize | 13.75 ms | 0.883 ms | 0.048 ms |
|  MemoryPackDeserialize | 19.33 ms | 1.040 ms | 0.057 ms |
|         CerasSerialize | 41.53 ms | 0.553 ms | 0.030 ms |
|       CerasDeserialize | 45.21 ms | 3.766 ms | 0.206 ms |
