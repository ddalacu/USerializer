``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 22.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=7.0.102
  [Host]   : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT
  ShortRun : .NET Core 7.0.2 (CoreCLR 7.0.222.60605, CoreFX 7.0.222.60605), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |   StdDev |
|----------------------- |----------:|----------:|---------:|
|   USerializerSerialize |  41.44 ms |  7.469 ms | 0.409 ms |
| USerializerDeserialize | 195.66 ms | 66.891 ms | 3.667 ms |
|    MemoryPackSerialize |  26.88 ms |  4.645 ms | 0.255 ms |
|  MemoryPackDeserialize |  35.45 ms | 19.463 ms | 1.067 ms |
|         CerasSerialize |  88.32 ms | 15.131 ms | 0.829 ms |
|       CerasDeserialize | 105.25 ms | 40.197 ms | 2.203 ms |
