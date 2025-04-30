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
|   USerializerSerialize | 12.86 ms | 2.242 ms | 0.123 ms |
| USerializerDeserialize | 90.95 ms | 8.619 ms | 0.472 ms |
|    MemoryPackSerialize | 16.29 ms | 0.040 ms | 0.002 ms |
|  MemoryPackDeserialize | 22.52 ms | 0.788 ms | 0.043 ms |
|         CerasSerialize | 50.58 ms | 5.387 ms | 0.295 ms |
|       CerasDeserialize | 58.99 ms | 1.487 ms | 0.082 ms |
