``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 24.04
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=9.0.203
  [Host]   : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT
  ShortRun : .NET Core 9.0.4 (CoreCLR 9.0.425.16305, CoreFX 9.0.425.16305), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|   USerializerSerialize |  9.242 ms | 1.4486 ms | 0.0794 ms |
| USerializerDeserialize | 26.595 ms | 4.5454 ms | 0.2491 ms |
|    MemoryPackSerialize | 14.639 ms | 0.7913 ms | 0.0434 ms |
|  MemoryPackDeserialize | 20.230 ms | 0.0899 ms | 0.0049 ms |
|         CerasSerialize | 43.328 ms | 8.7751 ms | 0.4810 ms |
|       CerasDeserialize | 47.068 ms | 3.7071 ms | 0.2032 ms |
