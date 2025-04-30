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
|   USerializerSerialize |  9.171 ms |  0.8493 ms | 0.0466 ms |
| USerializerDeserialize | 27.206 ms | 12.4210 ms | 0.6808 ms |
|    MemoryPackSerialize | 14.872 ms |  0.2454 ms | 0.0135 ms |
|  MemoryPackDeserialize | 20.809 ms |  4.4923 ms | 0.2462 ms |
|         CerasSerialize | 43.952 ms |  3.5329 ms | 0.1936 ms |
|       CerasDeserialize | 47.069 ms |  9.6353 ms | 0.5281 ms |
