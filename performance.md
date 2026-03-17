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
|   USerializerSerialize | 10.80 ms | 5.354 ms | 0.293 ms |
| USerializerDeserialize | 25.53 ms | 1.017 ms | 0.056 ms |
|    MemoryPackSerialize | 13.80 ms | 0.756 ms | 0.041 ms |
|  MemoryPackDeserialize | 20.37 ms | 2.511 ms | 0.138 ms |
|         CerasSerialize | 41.07 ms | 1.602 ms | 0.088 ms |
|       CerasDeserialize | 46.40 ms | 1.903 ms | 0.104 ms |
