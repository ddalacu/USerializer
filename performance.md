``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.102
  [Host]   : .NET Core 10.0.2 (CoreCLR 10.0.225.61305, CoreFX 10.0.225.61305), X64 RyuJIT
  ShortRun : .NET Core 10.0.2 (CoreCLR 10.0.225.61305, CoreFX 10.0.225.61305), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |   StdDev |
|----------------------- |---------:|---------:|---------:|
|   USerializerSerialize | 10.93 ms | 0.021 ms | 0.001 ms |
| USerializerDeserialize | 25.55 ms | 7.412 ms | 0.406 ms |
|    MemoryPackSerialize | 13.69 ms | 0.473 ms | 0.026 ms |
|  MemoryPackDeserialize | 19.42 ms | 1.855 ms | 0.102 ms |
|         CerasSerialize | 41.01 ms | 0.608 ms | 0.033 ms |
|       CerasDeserialize | 45.48 ms | 1.469 ms | 0.081 ms |
