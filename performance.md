``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 22.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=7.0.102
  [Host]   : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT
  ShortRun : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |    Error |   StdDev |
|----------------------- |----------:|---------:|---------:|
|   USerializerSerialize |  41.49 ms | 2.061 ms | 0.113 ms |
| USerializerDeserialize | 153.28 ms | 2.477 ms | 0.136 ms |
|    MemoryPackSerialize |  19.45 ms | 0.691 ms | 0.038 ms |
|  MemoryPackDeserialize |  29.41 ms | 5.074 ms | 0.278 ms |
|         CerasSerialize |  78.39 ms | 4.023 ms | 0.221 ms |
|       CerasDeserialize |  87.57 ms | 4.450 ms | 0.244 ms |
