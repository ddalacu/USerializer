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
|   USerializerSerialize | 10.84 ms | 3.398 ms | 0.186 ms |
| USerializerDeserialize | 25.81 ms | 2.971 ms | 0.163 ms |
|    MemoryPackSerialize | 13.84 ms | 0.328 ms | 0.018 ms |
|  MemoryPackDeserialize | 20.26 ms | 2.491 ms | 0.137 ms |
|         CerasSerialize | 41.67 ms | 0.127 ms | 0.007 ms |
|       CerasDeserialize | 46.78 ms | 1.529 ms | 0.084 ms |
