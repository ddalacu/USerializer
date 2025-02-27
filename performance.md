``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=7.0.410
  [Host]   : .NET Core 7.0.20 (CoreCLR 7.0.2024.26716, CoreFX 7.0.2024.26716), X64 RyuJIT
  ShortRun : .NET Core 7.0.20 (CoreCLR 7.0.2024.26716, CoreFX 7.0.2024.26716), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |   StdDev |
|----------------------- |----------:|----------:|---------:|
|   USerializerSerialize |  23.31 ms |  7.890 ms | 0.432 ms |
| USerializerDeserialize | 122.80 ms | 50.956 ms | 2.793 ms |
|    MemoryPackSerialize |  15.91 ms |  0.925 ms | 0.051 ms |
|  MemoryPackDeserialize |  20.38 ms |  3.105 ms | 0.170 ms |
|         CerasSerialize |  51.26 ms |  1.974 ms | 0.108 ms |
|       CerasDeserialize |  55.78 ms |  4.091 ms | 0.224 ms |
