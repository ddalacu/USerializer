``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=7.0.410
  [Host]   : .NET Core 7.0.20 (CoreCLR 7.0.2024.26716, CoreFX 7.0.2024.26716), X64 RyuJIT
  ShortRun : .NET Core 7.0.20 (CoreCLR 7.0.2024.26716, CoreFX 7.0.2024.26716), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |     Error |   StdDev |
|----------------------- |---------:|----------:|---------:|
|   USerializerSerialize | 22.59 ms |  0.826 ms | 0.045 ms |
| USerializerDeserialize | 88.60 ms | 19.368 ms | 1.062 ms |
|    MemoryPackSerialize | 16.26 ms |  7.270 ms | 0.399 ms |
|  MemoryPackDeserialize | 21.38 ms |  3.845 ms | 0.211 ms |
|         CerasSerialize | 50.19 ms |  3.080 ms | 0.169 ms |
|       CerasDeserialize | 55.82 ms |  6.400 ms | 0.351 ms |
