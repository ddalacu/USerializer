``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=10.0.201
  [Host]   : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT
  ShortRun : .NET Core 10.0.5 (CoreCLR 10.0.526.15411, CoreFX 10.0.526.15411), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |     Mean |    Error |   StdDev |
|----------------------- |---------:|---------:|---------:|
|   USerializerSerialize | 10.37 ms | 0.336 ms | 0.018 ms |
| USerializerDeserialize | 25.87 ms | 3.538 ms | 0.194 ms |
|    MemoryPackSerialize | 11.95 ms | 0.066 ms | 0.004 ms |
|  MemoryPackDeserialize | 19.63 ms | 0.624 ms | 0.034 ms |
|         CerasSerialize | 44.17 ms | 0.547 ms | 0.030 ms |
|       CerasDeserialize | 45.11 ms | 4.073 ms | 0.223 ms |
