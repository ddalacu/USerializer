``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=9.0.203
  [Host]   : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT
  ShortRun : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |      Error |    StdDev |
|----------------------- |----------:|-----------:|----------:|
|   USerializerSerialize |  9.308 ms |  0.5464 ms | 0.0299 ms |
| USerializerDeserialize | 27.472 ms |  4.7040 ms | 0.2578 ms |
|    MemoryPackSerialize | 15.257 ms |  1.0727 ms | 0.0588 ms |
|  MemoryPackDeserialize | 21.412 ms |  3.6035 ms | 0.1975 ms |
|         CerasSerialize | 47.219 ms | 13.8595 ms | 0.7597 ms |
|       CerasDeserialize | 47.642 ms |  1.5252 ms | 0.0836 ms |
