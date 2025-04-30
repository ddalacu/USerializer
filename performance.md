``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=7.0.410
  [Host]   : .NET Core 7.0.20 (CoreCLR 7.0.2024.26716, CoreFX 7.0.2024.26716), X64 RyuJIT
  ShortRun : .NET Core 7.0.20 (CoreCLR 7.0.2024.26716, CoreFX 7.0.2024.26716), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |   StdDev |
|----------------------- |---------:|---------:|---------:|
|   USerializerSerialize | 12.81 ms | 0.088 ms | 0.005 ms |
| USerializerDeserialize | 32.99 ms | 3.784 ms | 0.207 ms |
|    MemoryPackSerialize | 16.24 ms | 0.901 ms | 0.049 ms |
|  MemoryPackDeserialize | 20.91 ms | 2.725 ms | 0.149 ms |
|         CerasSerialize | 54.75 ms | 2.991 ms | 0.164 ms |
|       CerasDeserialize | 55.97 ms | 2.468 ms | 0.135 ms |
