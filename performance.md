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
|   USerializerSerialize | 10.55 ms | 0.790 ms | 0.043 ms |
| USerializerDeserialize | 25.82 ms | 3.494 ms | 0.192 ms |
|    MemoryPackSerialize | 13.77 ms | 0.230 ms | 0.013 ms |
|  MemoryPackDeserialize | 19.27 ms | 1.717 ms | 0.094 ms |
|         CerasSerialize | 41.40 ms | 0.416 ms | 0.023 ms |
|       CerasDeserialize | 45.41 ms | 1.704 ms | 0.093 ms |
