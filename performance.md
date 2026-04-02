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
|   USerializerSerialize | 10.49 ms | 0.619 ms | 0.034 ms |
| USerializerDeserialize | 25.78 ms | 4.058 ms | 0.222 ms |
|    MemoryPackSerialize | 13.68 ms | 0.874 ms | 0.048 ms |
|  MemoryPackDeserialize | 25.13 ms | 6.417 ms | 0.352 ms |
|         CerasSerialize | 41.53 ms | 3.303 ms | 0.181 ms |
|       CerasDeserialize | 46.23 ms | 4.186 ms | 0.229 ms |
