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
|   USerializerSerialize |  9.179 ms |  0.1770 ms | 0.0097 ms |
| USerializerDeserialize | 25.908 ms |  9.8215 ms | 0.5383 ms |
|    MemoryPackSerialize | 14.821 ms |  0.1305 ms | 0.0072 ms |
|  MemoryPackDeserialize | 21.085 ms |  3.3755 ms | 0.1850 ms |
|         CerasSerialize | 42.681 ms | 18.9107 ms | 1.0366 ms |
|       CerasDeserialize | 47.979 ms | 10.6870 ms | 0.5858 ms |
