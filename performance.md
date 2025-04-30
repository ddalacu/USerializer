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
|   USerializerSerialize | 12.95 ms |  2.709 ms | 0.148 ms |
| USerializerDeserialize | 90.18 ms | 56.067 ms | 3.073 ms |
|    MemoryPackSerialize | 16.26 ms |  0.523 ms | 0.029 ms |
|  MemoryPackDeserialize | 21.30 ms |  1.165 ms | 0.064 ms |
|         CerasSerialize | 50.27 ms |  1.348 ms | 0.074 ms |
|       CerasDeserialize | 54.14 ms |  1.101 ms | 0.060 ms |
